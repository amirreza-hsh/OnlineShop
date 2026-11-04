using OnlineShop.Domain.Enums;

namespace OnlineShop.Domain.Rules;

// قوانین تغییر وضعیت سفارش در عملیات آماده‌سازی و ارسال مدیر
// این کلاس به دیتابیس یا درخواست HTTP وابسته نیست
public static class OrderStatusRules
{
    public static bool CanAdminTransition(
        OrderStatus currentStatus,
        OrderStatus nextStatus)
    {
        // هر جفت شامل وضعیت فعلی و وضعیت درخواستی است
        // فقط سه جفت زیر مجاز هستند
        return (currentStatus, nextStatus) switch
        {
            // سفارش پرداخت‌شده وارد مرحله آماده‌سازی می‌شود
            (OrderStatus.Paid, OrderStatus.Processing) => true,

            // سفارش آماده‌شده وارد مرحله ارسال می‌شود
            (OrderStatus.Processing, OrderStatus.Shipped) => true,

            // سفارش ارسال‌شده می‌تواند تحویل‌شده ثبت شود
            (OrderStatus.Shipped, OrderStatus.Delivered) => true,

            // تمام تغییرهای دیگر در این عملیات ممنوع هستند
            _ => false
        };
    }
}