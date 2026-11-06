using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Enums;
using OnlineShop.Infrastructure.Data;
using OnlineShop.Infrastructure.Identity;
using OnlineShop.Infrastructure.Orders;
using Xunit;

namespace OnlineShop.Tests;

public class OrderStatusPersistenceTests
{
    // هر ردیف یک آزمایش جدا با دیتابیس موقت خودش است
    [Theory]
    [InlineData(OrderStatus.Paid, OrderStatus.Processing,
        true, OrderStatus.Processing)]
    [InlineData(OrderStatus.PendingPayment, OrderStatus.Processing,
        false, OrderStatus.PendingPayment)]
    [InlineData(OrderStatus.Paid, OrderStatus.Shipped,
        false, OrderStatus.Paid)]
    public async Task ChangeStatusForAdminAsync_PersistsOnlyAllowedChanges(
        OrderStatus initialStatus,
        OrderStatus requestedStatus,
        bool expectedChanged,
        OrderStatus expectedStoredStatus)
    {
        // دیتابیس فقط در حافظه است و به اطلاعات فروشگاه وصل نیست
        // تا پایان تست اتصال را باز نگه می‌داریم
        await using var connection = new SqliteConnection(
            "Data Source=:memory:;Foreign Keys=True");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        int orderId;

        // آماده‌سازی جدول‌ها و داده‌های آزمایشی
        await using (var setupContext = new ApplicationDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();

            // سفارش باید به یک کاربر موجود تعلق داشته باشد
            // برای این تست نیازی به رمز یا ورود کاربر نداریم
            var customer = new ApplicationUser
            {
                Id = "test-customer",
                UserName = "test-customer",
                FullName = "مشتری آزمایشی"
            };

            setupContext.Users.Add(customer);

            // وضعیت اولیه فقط برای آماده‌سازی سناریوی تست تنظیم می‌شود
            var order = new Order
            {
                UserId = customer.Id,
                RequestId = Guid.NewGuid(),
                Status = initialStatus,
                TotalAmount = 450_000m,
                RecipientName = "مشتری آزمایشی",
                PhoneNumber = "09123456789",
                Province = "تهران",
                City = "تهران",
                Address = "آدرس آزمایشی",
                PostalCode = "1234567890"
            };

            setupContext.Orders.Add(order);
            await setupContext.SaveChangesAsync();

            // شماره سفارش بعد از ذخیره مشخص می‌شود
            orderId = order.Id;
        }

        // اجرای همان سرویس واقعی که کنترلر مدیر فراخوانی می‌کند
        await using (var serviceContext = new ApplicationDbContext(options))
        {
            var service = new OrderService(serviceContext);

            bool changed = await service.ChangeStatusForAdminAsync(
                orderId,
                requestedStatus);

            Assert.Equal(expectedChanged, changed);
        }

        // با یک context تازه از دیتابیس می‌خوانیم
        // نتیجه باید واقعاً ذخیره شده باشد
        await using var verifyContext = new ApplicationDbContext(options);

        OrderStatus storedStatus = await verifyContext.Orders
            .Where(order => order.Id == orderId)
            .Select(order => order.Status)
            .SingleAsync();

        Assert.Equal(expectedStoredStatus, storedStatus);
    }
}