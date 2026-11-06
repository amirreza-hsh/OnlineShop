using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Enums;
using OnlineShop.Infrastructure.Data;
using OnlineShop.Infrastructure.Identity;
using OnlineShop.Infrastructure.Payments;
using Xunit;

namespace OnlineShop.Tests;

public class SimulatedPaymentTests
{
    // موجودی محصول دوم برای سفارش کافی است
    [Theory]
    [InlineData(4, true)]

    // موجودی محصول دوم کافی نیست و تغییر محصول اول باید برگردد
    [InlineData(1, false)]
    public async Task ConfirmAsync_ChangesStockAndOrderTogether(
        int secondProductStock,
        bool shouldSucceed)
    {
        // هر سناریو دیتابیس موقت خودش را دارد
        await using var connection = new SqliteConnection(
            "Data Source=:memory:;Foreign Keys=True");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        const string customerId = "payment-test-customer";
        int orderId;

        await using (var setup = new ApplicationDbContext(options))
        {
            await setup.Database.EnsureCreatedAsync();

            setup.Users.Add(new ApplicationUser
            {
                Id = customerId,
                UserName = customerId,
                FullName = "مشتری آزمایشی"
            });

            // دسته‌بندی شماره یک از داده نمونه پروژه می‌آید
            // محصول شماره ده در سرویس قبل از شماره بیست بررسی می‌شود
            var first = new Product
            {
                Id = 10,
                Title = "بازی اول",
                CategoryId = 1,
                Price = 100_000m,
                Stock = 5
            };

            var second = new Product
            {
                Id = 20,
                Title = "بازی دوم",
                CategoryId = 1,
                Price = 100_000m,
                Stock = secondProductStock
            };

            setup.Products.AddRange(first, second);

            var order = new Order
            {
                UserId = customerId,
                RequestId = Guid.NewGuid(),
                Status = OrderStatus.PendingPayment,
                TotalAmount = 400_000m,
                RecipientName = "مشتری آزمایشی",
                PhoneNumber = "09123456789",
                Province = "تهران",
                City = "تهران",
                Address = "آدرس آزمایشی",
                PostalCode = "1234567890",
                Items = new List<OrderItem>
                {
                    new()
                    {
                        ProductId = first.Id,
                        ProductName = first.Title,
                        UnitPrice = first.Price,
                        Quantity = 2
                    },
                    new()
                    {
                        ProductId = second.Id,
                        ProductName = second.Title,
                        UnitPrice = second.Price,
                        Quantity = 2
                    }
                }
            };

            setup.Orders.Add(order);
            await setup.SaveChangesAsync();
            orderId = order.Id;
        }

        await using (var firstAttempt = new ApplicationDbContext(options))
        {
            var service = new SimulatedPaymentService(firstAttempt);

            if (shouldSucceed)
            {
                var payment = await service.ConfirmAsync(
                    customerId,
                    orderId);

                // درخواست دوم را با context تازه اجرا می‌کنیم
                await using var retry = new ApplicationDbContext(options);
                var retryService = new SimulatedPaymentService(retry);

                var repeated = await retryService.ConfirmAsync(
                    customerId,
                    orderId);

                Assert.Equal(payment.Reference, repeated.Reference);
            }
            else
            {
                // کمبود موجودی باید با خطای مشخص پرداخت گزارش شود
                await Assert.ThrowsAsync<SimulatedPaymentConflictException>(
                    () => service.ConfirmAsync(customerId, orderId));
            }
        }

        // با context تازه نتیجه ذخیره‌شده را بررسی می‌کنیم
        await using var verify = new ApplicationDbContext(options);

        var storedOrder = await verify.Orders.SingleAsync();
        var products = await verify.Products.OrderBy(p => p.Id).ToListAsync();
        var payments = await verify.SimulatedPayments.ToListAsync();

        if (shouldSucceed)
        {
            Assert.Equal(OrderStatus.Paid, storedOrder.Status);
            Assert.Equal(3, products[0].Stock);
            Assert.Equal(2, products[1].Stock);
            Assert.Equal(400_000m, Assert.Single(payments).Amount);
        }
        else
        {
            Assert.Equal(OrderStatus.PendingPayment, storedOrder.Status);
            Assert.Equal(5, products[0].Stock);
            Assert.Equal(1, products[1].Stock);
            Assert.Empty(payments);
        }
    }
}