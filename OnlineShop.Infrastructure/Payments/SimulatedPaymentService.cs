using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Enums;
using OnlineShop.Infrastructure.Data;

namespace OnlineShop.Infrastructure.Payments;

public class SimulatedPaymentService
{
    private readonly ApplicationDbContext _context;

    public SimulatedPaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SimulatedPayment> ConfirmAsync(
        string userId,
        int orderId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("شناسه کاربر معتبر نیست");
        }

        // تغییر وضعیت و موجودی و ثبت پرداخت باید با هم قطعی شوند
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                cancellationToken);

        // شماره سفارش به‌تنهایی برای دسترسی کافی نیست
        var ownedOrder = _context.Orders.Where(order =>
            order.Id == orderId && order.UserId == userId);

        // فقط یک درخواست می‌تواند سفارش منتظر پرداخت را تغییر دهد
        // این تغییر هنوز داخل تراکنش است و قطعی نشده است
        int updated = await ownedOrder
            .Where(order => order.Status == OrderStatus.PendingPayment)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    order => order.Status,
                    OrderStatus.Paid),
                cancellationToken);

        var order = await ownedOrder
            .AsNoTracking()
            .Include(order => order.Items)
            .SingleOrDefaultAsync(cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException("سفارش یافت نشد");
        }

        if (updated == 0)
        {
            // اگر قبلاً تأیید شده باشد همان رکورد را برمی‌گردانیم
            // در این مسیر هیچ تغییری در موجودی انجام نمی‌شود
            var previous = await _context.SimulatedPayments
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    payment => payment.OrderId == orderId,
                    cancellationToken);

            if (previous != null)
            {
                return previous;
            }

            // برای مثال سفارش لغوشده قابل تأیید نیست
            throw new SimulatedPaymentConflictException(
                "وضعیت سفارش اجازه تأیید پرداخت را نمی‌دهد");
        }

        if (order.Items.Count == 0 ||
            order.Items.Any(item => item.Quantity <= 0))
        {
            throw new SimulatedPaymentConflictException(
                $"محصول {item.ProductId} موجودی کافی ندارد یا فعال نیست");
        }

        // هر محصول فقط یک بار و با مجموع تعداد ردیف‌هایش کم می‌شود
        var quantities = order.Items
            .GroupBy(item => item.ProductId)
            .OrderBy(group => group.Key)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            });

        foreach (var item in quantities)
        {
            // شرط موجودی داخل همان دستور کاهش بررسی می‌شود
            // خواندن موجودی و کم کردن آن در دو فرمان جدا انجام نمی‌شود
            int changed = await _context.Products
                .Where(product =>
                    product.Id == item.ProductId &&
                    product.IsActive &&
                    product.Stock >= item.Quantity)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        product => product.Stock,
                        product => product.Stock - item.Quantity),
                    cancellationToken);

            if (changed != 1)
            {
                // خروج بدون Commit باعث برگشت تغییرات تراکنش می‌شود
                throw new InvalidOperationException(
                    $"محصول {item.ProductId} موجودی کافی ندارد یا فعال نیست");
            }
        }

        // مبلغ پرداخت را از سفارش ذخیره‌شده برمی‌داریم
        var payment = new SimulatedPayment
        {
            OrderId = order.Id,
            Reference = Guid.NewGuid(),
            Amount = order.TotalAmount,
            ConfirmedAtUtc = DateTime.UtcNow
        };

        _context.SimulatedPayments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        // فقط بعد از موفقیت همه مراحل تغییرات را قطعی می‌کنیم
        await transaction.CommitAsync(cancellationToken);

        return payment;
    }
}