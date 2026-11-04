using OnlineShop.Domain.Enums;
using OnlineShop.Domain.Rules;
using Xunit;

namespace OnlineShop.Tests;

public class OrderStatusRulesTests
{
    // یک تست را با سه جفت وضعیت مجاز اجرا می‌کنیم
    [Theory]
    [InlineData(OrderStatus.Paid, OrderStatus.Processing)]
    [InlineData(OrderStatus.Processing, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Delivered)]
    public void CanAdminTransition_AllowedTransition_ReturnsTrue(
        OrderStatus currentStatus,
        OrderStatus nextStatus)
    {
        // اجرای قانون با وضعیت‌های ورودی
        bool allowed = OrderStatusRules.CanAdminTransition(
            currentStatus,
            nextStatus);

        // برای این تغییرهای مجاز انتظار true داریم
        Assert.True(allowed);
    }

    // سفارش پرداخت‌نشده نباید وارد آماده‌سازی شود
    [Theory]
    [InlineData(OrderStatus.PendingPayment, OrderStatus.Processing)]

    // مدیر از این عملیات نمی‌تواند پرداخت را تأیید کند
    [InlineData(OrderStatus.PendingPayment, OrderStatus.Paid)]

    // عبور مستقیم از پرداخت به ارسال مجاز نیست
    [InlineData(OrderStatus.Paid, OrderStatus.Shipped)]

    // برگشت به مرحله قبلی مجاز نیست
    [InlineData(OrderStatus.Shipped, OrderStatus.Processing)]

    // سفارش تحویل‌شده نباید دوباره ارسال شود
    [InlineData(OrderStatus.Delivered, OrderStatus.Shipped)]

    // سفارش لغوشده نباید وارد آماده‌سازی شود
    [InlineData(OrderStatus.Cancelled, OrderStatus.Processing)]

    // سفارش منقضی‌شده نباید وارد آماده‌سازی شود
    [InlineData(OrderStatus.Expired, OrderStatus.Processing)]

    // ثابت‌ماندن وضعیت یک حرکت به مرحله بعد محسوب نمی‌شود
    [InlineData(OrderStatus.Processing, OrderStatus.Processing)]

    // مقدار عددی خارج از وضعیت‌های تعریف‌شده هم مجاز نیست
    [InlineData((OrderStatus)999, OrderStatus.Processing)]
    public void CanAdminTransition_DisallowedTransition_ReturnsFalse(
        OrderStatus currentStatus,
        OrderStatus nextStatus)
    {
        // اجرای همان قانون برای یک تغییر غیرمجاز
        bool allowed = OrderStatusRules.CanAdminTransition(
            currentStatus,
            nextStatus);

        // قانون باید درخواست را رد کند
        Assert.False(allowed);
    }
}