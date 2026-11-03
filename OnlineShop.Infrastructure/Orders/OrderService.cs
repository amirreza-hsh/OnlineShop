using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.DTOs.Orders;
using OnlineShop.Application.Interfaces;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Enums;
using OnlineShop.Infrastructure.Data;

namespace OnlineShop.Infrastructure.Orders;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> CreateOrderAsync(
        string userId,
        CreateOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("شناسه کاربر معتبر نیست");

        // Guid خالی یعنی مشتری شناسه معتبری برای درخواست نفرستاده
        if (dto.RequestId == Guid.Empty)
            throw new ArgumentException("شناسه درخواست سفارش الزامی است");

        // قوانین DTO را هنگام فراخوانی مستقیم سرویس هم بررسی می‌کنیم
        var validationResults = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(
            dto,
            new ValidationContext(dto),
            validationResults,
            validateAllProperties: true);

        if (!isValid)
        {
            throw new ArgumentException(
                string.Join(" | ",
                    validationResults.Select(error => error.ErrorMessage)));
        }

        // اگر همین درخواست قبلاً موفق بوده، همان سفارش را برمی‌گردانیم
        // بنابراین قطع ارتباط و تکرار درخواست، سفارش جدید نمی‌سازد
        var existing = await FindByRequestAsync(
            userId,
            dto.RequestId,
            cancellationToken);

        if (existing != null)
            return MapToDto(existing);

        // ثبت سفارش و بستن سبد باید یک عملیات هماهنگ باشند
        // اگر عملیات قبل از Commit شکست بخورد، تغییرات برمی‌گردند
        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // فقط شناسه سبد فعال همین حساب را می‌خوانیم
            // اینجا خود سبد را وارد ChangeTracker نمی‌کنیم
            int? cartId = await _context.Carts
                .Where(cart => cart.UserId == userId && cart.IsActive)
                .Select(cart => (int?)cart.Id)
                .SingleOrDefaultAsync(cancellationToken);

            if (cartId == null)
            {
                // ممکن است درخواست هم‌زمان قبلی، همین سفارش را ثبت کرده باشد
                existing = await FindByRequestAsync(
                    userId,
                    dto.RequestId,
                    cancellationToken);

                if (existing != null)
                    return MapToDto(existing);

                throw new ArgumentException("سبد خرید فعال وجود ندارد");
            }

            // سبد را با یک دستور شرطی غیرفعال می‌کنیم
            // فقط یکی از درخواست‌های هم‌زمان می‌تواند سبد فعال را ببندد
            // این تغییر هنوز داخل تراکنش است و در صورت خطا برمی‌گردد
            int claimed = await _context.Carts
                .Where(cart =>
                    cart.Id == cartId.Value &&
                    cart.UserId == userId &&
                    cart.IsActive)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(cart => cart.IsActive, false)
                        .SetProperty(
                            cart => cart.UpdatedAt,
                            (DateTime?)DateTime.UtcNow),
                    cancellationToken);

            if (claimed == 0)
            {
                existing = await FindByRequestAsync(
                    userId,
                    dto.RequestId,
                    cancellationToken);

                if (existing != null)
                    return MapToDto(existing);

                throw new ArgumentException(
                    "این سبد قبلاً برای ثبت سفارش استفاده شده است");
            }

            // اقلام را همراه اطلاعات فعلی محصول می‌خوانیم
            // قیمت ثبت‌شده در سبد را مبنای مبلغ سفارش قرار نمی‌دهیم
            var cartItems = await _context.CartItems
                .AsNoTracking()
                .Where(item => item.CartId == cartId.Value)
                .Include(item => item.Product)
                .OrderBy(item => item.Id)
                .ToListAsync(cancellationToken);

            if (cartItems.Count == 0)
                throw new ArgumentException("سبد خرید خالی است");

            var order = new Order
            {
                UserId = userId,
                RequestId = dto.RequestId,
                Status = OrderStatus.PendingPayment,
                CreatedAt = DateTime.UtcNow,

                // اطلاعات گیرنده را به‌صورت مستقل داخل سفارش نگه می‌داریم
                RecipientName = dto.RecipientName.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                Province = dto.Province.Trim(),
                City = dto.City.Trim(),
                Address = dto.Address.Trim(),
                PostalCode = dto.PostalCode.Trim()
            };

            // اگر داده قدیمی شامل ردیف تکراری یک محصول باشد،
            // تعدادهای آن را با هم بررسی می‌کنیم
            foreach (var group in cartItems.GroupBy(item => item.ProductId))
            {
                var product = group.First().Product;

                if (product == null || !product.IsActive)
                    throw new ArgumentException(
                        "یکی از محصولات سبد دیگر قابل خرید نیست");

                if (group.Any(item => item.Quantity < 1))
                    throw new ArgumentException(
                        "تعداد یکی از اقلام سبد معتبر نیست");

                long quantity = group.Sum(item => (long)item.Quantity);

                if (quantity > 100)
                    throw new ArgumentException(
                        "تعداد هر محصول نمی‌تواند بیشتر از ۱۰۰ باشد");

                if (quantity > product.Stock)
                    throw new ArgumentException(
                        $"موجودی محصول {product.Title} کافی نیست");

                // قیمت فعلی محصول یا قیمت تخفیف‌خورده آن را انتخاب می‌کنیم
                decimal unitPrice = product.DiscountPrice ?? product.Price;

                // داده نامعتبر محصول نباید وارد سفارش شود
                if (product.Price <= 0 ||
                    unitPrice < 0 ||
                    (product.DiscountPrice.HasValue &&
                     product.DiscountPrice.Value >= product.Price))
                {
                    throw new ArgumentException(
                        $"قیمت محصول {product.Title} معتبر نیست");
                }

                // نام و قیمت زمان ثبت سفارش را کپی می‌کنیم
                // تغییر آینده محصول، این اطلاعات را تغییر نمی‌دهد
                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Title,
                    UnitPrice = unitPrice,
                    Quantity = (int)quantity
                });
            }

            // در نسخه فعلی مبلغ فقط مجموع اقلام است
            order.TotalAmount = order.Items.Sum(
                item => item.UnitPrice * item.Quantity);

            _context.Orders.Add(order);

            await _context.SaveChangesAsync(cancellationToken);

            // بعد از موفقیت تمام مراحل، تغییرات را قطعی می‌کنیم
            await transaction.CommitAsync(cancellationToken);

            return MapToDto(order);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqlException sql &&
                  (sql.Number == 2601 || sql.Number == 2627) &&
                  sql.Message.Contains(
                      "IX_Orders_UserId_RequestId",
                      StringComparison.OrdinalIgnoreCase))
        {
            // اگر دو درخواست هم‌زمان به محدودیت شناسه تکراری برخورد کنند،
            // تغییرات این درخواست را برمی‌گردانیم
            await transaction.RollbackAsync(CancellationToken.None);

            // داده‌های ذخیره‌نشده این عملیات را از ردیابی خارج می‌کنیم
            _context.ChangeTracker.Clear();

            existing = await FindByRequestAsync(
                userId,
                dto.RequestId,
                cancellationToken);

            if (existing != null)
                return MapToDto(existing);

            throw;
        }
    }

    public async Task<List<OrderSummaryDto>> GetMyOrdersAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // سرویس ممکن است مستقیماً از تست یا بخش دیگری فراخوانی شود
        // بنابراین شناسه کاربر را همین‌جا هم بررسی می‌کنیم
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("شناسه کاربر معتبر نیست");
        }

        // شماره صفحه از یک شروع می‌شود
        if (page < 1)
        {
            throw new ArgumentException("شماره صفحه باید حداقل یک باشد");
        }

        // برای جلوگیری از دریافت حجم زیاد اطلاعات محدودیت می‌گذاریم
        if (pageSize < 1 || pageSize > 50)
        {
            throw new ArgumentException(
                "تعداد سفارش‌های هر صفحه باید بین ۱ و ۵۰ باشد");
        }

        // محاسبه تعداد سفارش‌هایی که باید از ابتدای فهرست رد کنیم
        // استفاده از long مانع سرریز هنگام ضرب ورودی‌های بزرگ می‌شود
        long skip = ((long)page - 1) * pageSize;

        // متد Skip تعداد را به شکل int دریافت می‌کند
        if (skip > int.MaxValue)
        {
            throw new ArgumentException("شماره صفحه بیش از حد بزرگ است");
        }

        return await _context.Orders
            // این درخواست فقط اطلاعات را می‌خواند
            .AsNoTracking()

            // فقط سفارش‌های حساب واردشده وارد نتیجه می‌شوند
            .Where(order => order.UserId == userId)

            // سفارش‌های جدیدتر بالاتر نمایش داده می‌شوند
            .OrderByDescending(order => order.CreatedAt)

            // اگر زمان دو سفارش برابر بود ترتیب همچنان مشخص می‌ماند
            .ThenByDescending(order => order.Id)

            // سفارش‌های صفحات قبلی را رد می‌کنیم
            .Skip((int)skip)

            // فقط تعداد موردنیاز همین صفحه را دریافت می‌کنیم
            .Take(pageSize)

            // فقط ستون‌های موردنیاز فهرست از دیتابیس خوانده می‌شوند
            // برای این صفحه نیازی به خواندن اقلام سفارش نداریم
            .Select(order => new OrderSummaryDto
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                TotalAmount = order.TotalAmount
            })

            // اجرای درخواست دیتابیس به شکل غیرهم‌زمان
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(
        string userId,
        int orderId,
        CancellationToken cancellationToken = default)
    {
        // شناسه سفارش به‌تنهایی کافی نیست
        // شرط مالکیت هم داخل همان جست‌وجوی دیتابیس قرار دارد
        var order = await _context.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .SingleOrDefaultAsync(
                order => order.Id == orderId && order.UserId == userId,
                cancellationToken);

        return order == null ? null : MapToDto(order);
    }

    public async Task<bool> CancelOrderAsync(
   string userId,
   int orderId,
   CancellationToken cancellationToken = default)
    {
        // شناسه کاربر باید از هویت معتبر درخواست دریافت شده باشد
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("شناسه کاربر معتبر نیست");
        }

        // مالکیت و وضعیت را هم‌زمان در دستور تغییر بررسی می‌کنیم
        // تنها سفارش پرداخت‌نشده متعلق به همین کاربر تغییر می‌کند
        int affectedRows = await _context.Orders
            .Where(order =>
                order.Id == orderId &&
                order.UserId == userId &&
                order.Status == OrderStatus.PendingPayment)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    order => order.Status,
                    OrderStatus.Cancelled),
                cancellationToken);

        // تغییر یک ردیف یعنی لغو سفارش موفق بوده است
        if (affectedRows == 1)
        {
            return true;
        }

        // اگر ردیفی تغییر نکرد دلیل را بررسی می‌کنیم
        // فقط وضعیت سفارش همین کاربر را می‌خوانیم
        OrderStatus? currentStatus = await _context.Orders
            .AsNoTracking()
            .Where(order =>
                order.Id == orderId &&
                order.UserId == userId)
            .Select(order => (OrderStatus?)order.Status)
            .SingleOrDefaultAsync(cancellationToken);

        // nullable بودن کمک می‌کند نبودن سفارش را تشخیص بدهیم
        // سفارش دیگران هم برای این کاربر یافت‌نشده محسوب می‌شود
        if (currentStatus == null)
        {
            throw new KeyNotFoundException("سفارش یافت نشد");
        }

        // تکرار درخواست لغو برای سفارش لغوشده موفق محسوب می‌شود
        if (currentStatus == OrderStatus.Cancelled)
        {
            return true;
        }

        // سفارش وجود دارد اما وضعیت آن اجازه لغو نمی‌دهد
        return false;
    }
    public async Task<List<OrderSummaryDto>> GetOrdersForAdminAsync(
    int page,
    int pageSize,
    CancellationToken cancellationToken = default)
    {
        // شماره صفحه از یک شروع می‌شود
        if (page < 1)
        {
            throw new ArgumentException(
                "شماره صفحه باید حداقل یک باشد");
        }

        // مدیر هم نباید همه سفارش‌ها را در یک درخواست دریافت کند
        if (pageSize < 1 || pageSize > 50)
        {
            throw new ArgumentException(
                "تعداد سفارش‌های هر صفحه باید بین ۱ و ۵۰ باشد");
        }

        // تعداد ردیف‌های صفحات قبلی را محاسبه می‌کنیم
        // long از سرریز حاصل ضرب ورودی‌های بزرگ جلوگیری می‌کند
        long skip = ((long)page - 1) * pageSize;

        if (skip > int.MaxValue)
        {
            throw new ArgumentException(
                "شماره صفحه بیش از حد بزرگ است");
        }

        return await _context.Orders
            // فقط اطلاعات را می‌خوانیم و تغییری در سفارش‌ها نمی‌دهیم
            .AsNoTracking()

            // این متد مخصوص مدیریت است و سفارش‌های همه کاربران را می‌خواند
            // مجوز دسترسی به آن در کنترلر بررسی می‌شود
            .OrderByDescending(order => order.CreatedAt)

            // برای زمان‌های برابر هم ترتیب مشخصی داریم
            .ThenByDescending(order => order.Id)

            // فقط سفارش‌های صفحه موردنظر را دریافت می‌کنیم
            .Skip((int)skip)
            .Take(pageSize)

            // برای فهرست به خلاصه سفارش نیاز داریم
            // اطلاعات گیرنده و محصولات از مسیر جزئیات خوانده می‌شوند
            .Select(order => new OrderSummaryDto
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                TotalAmount = order.TotalAmount
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderDto?> GetOrderForAdminAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        // مدیر برای رسیدگی به سفارش باید اقلام آن را هم ببیند
        var order = await _context.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .SingleOrDefaultAsync(
                order => order.Id == orderId,
                cancellationToken);

        // سفارش ناموجود با null مشخص می‌شود
        // تبدیل به پاسخ ۴۰۴ بر عهده کنترلر است
        if (order == null)
        {
            return null;
        }

        // از تبدیل‌کننده موجود پروژه دوباره استفاده می‌کنیم
        return MapToDto(order);
    }
    public async Task<List<OrderSummaryDto>> GetOrdersForAdminAsync(
    int page,
    int pageSize,
    CancellationToken cancellationToken = default)
    {
        // شماره صفحه از یک شروع می‌شود
        if (page < 1)
        {
            throw new ArgumentException(
                "شماره صفحه باید حداقل یک باشد");
        }

        // مدیر هم نباید همه سفارش‌ها را در یک درخواست دریافت کند
        if (pageSize < 1 || pageSize > 50)
        {
            throw new ArgumentException(
                "تعداد سفارش‌های هر صفحه باید بین ۱ و ۵۰ باشد");
        }

        // تعداد ردیف‌های صفحات قبلی را محاسبه می‌کنیم
        // long از سرریز حاصل ضرب ورودی‌های بزرگ جلوگیری می‌کند
        long skip = ((long)page - 1) * pageSize;

        if (skip > int.MaxValue)
        {
            throw new ArgumentException(
                "شماره صفحه بیش از حد بزرگ است");
        }

        return await _context.Orders
            // فقط اطلاعات را می‌خوانیم و تغییری در سفارش‌ها نمی‌دهیم
            .AsNoTracking()

            // این متد مخصوص مدیریت است و سفارش‌های همه کاربران را می‌خواند
            // مجوز دسترسی به آن در کنترلر بررسی می‌شود
            .OrderByDescending(order => order.CreatedAt)

            // برای زمان‌های برابر هم ترتیب مشخصی داریم
            .ThenByDescending(order => order.Id)

            // فقط سفارش‌های صفحه موردنظر را دریافت می‌کنیم
            .Skip((int)skip)
            .Take(pageSize)

            // برای فهرست به خلاصه سفارش نیاز داریم
            // اطلاعات گیرنده و محصولات از مسیر جزئیات خوانده می‌شوند
            .Select(order => new OrderSummaryDto
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                TotalAmount = order.TotalAmount
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderDto?> GetOrderForAdminAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        // مدیر برای رسیدگی به سفارش باید اقلام آن را هم ببیند
        var order = await _context.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .SingleOrDefaultAsync(
                order => order.Id == orderId,
                cancellationToken);

        // سفارش ناموجود با null مشخص می‌شود
        // تبدیل به پاسخ ۴۰۴ بر عهده کنترلر است
        if (order == null)
        {
            return null;
        }

        // از تبدیل‌کننده موجود پروژه دوباره استفاده می‌کنیم
        return MapToDto(order);
    }
    private Task<Order?> FindByRequestAsync(
        string userId,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        // تکرار درخواست فقط در محدوده همان حساب بررسی می‌شود
        return _context.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .SingleOrDefaultAsync(
                order =>
                    order.UserId == userId &&
                    order.RequestId == requestId,
                cancellationToken);
    }

    private static OrderDto MapToDto(Order order)
    {
        // مدل دیتابیس را به پاسخ محدود و مشخص API تبدیل می‌کنیم
        return new OrderDto
        {
            Id = order.Id,
            CreatedAt = order.CreatedAt,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            RecipientName = order.RecipientName,
            PhoneNumber = order.PhoneNumber,
            Province = order.Province,
            City = order.City,
            Address = order.Address,
            PostalCode = order.PostalCode,

            Items = order.Items
                .OrderBy(item => item.Id)
                .Select(item => new OrderItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity
                })
                .ToList()
        };
    }
}
