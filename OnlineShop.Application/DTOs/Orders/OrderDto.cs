using OnlineShop.Domain.Enums;

namespace OnlineShop.Application.DTOs.Orders;

// اطلاعات قابل نمایش یک سفارش
// مدل دیتابیس را مستقیماً به مشتری برنمی‌گردانیم
public class OrderDto
{
    // شناسه سفارش برای دریافت جزئیات آن استفاده می‌شود
    public int Id { get; set; }

    // زمان ثبت سفارش که در مدل اصلی با UTC نگهداری می‌شود
    public DateTime CreatedAt { get; set; }

    // وضعیت ذخیره‌شده سفارش
    // بدون تنظیم اضافی JSON، این enum به شکل عدد ارسال می‌شود
    public OrderStatus Status { get; set; }

    // نام وضعیت را هم برای خوانایی پاسخ برمی‌گردانیم
    // این متن نام فنی وضعیت است و ترجمه فارسی نیست
    public string StatusName => Status.ToString();

    // مبلغی که سرور برای سفارش ثبت کرده است
    // این مقدار از درخواست ثبت سفارش مشتری دریافت نمی‌شود
    public decimal TotalAmount { get; set; }

    // اطلاعات گیرنده از نسخه ثبت‌شده در سفارش خوانده می‌شود
    public string RecipientName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Province { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    // فهرست خالی اولیه از null بودن Items جلوگیری می‌کند
    // هنگام تبدیل سفارش به DTO، ردیف‌های واقعی را داخل آن می‌گذاریم
    public List<OrderItemDto> Items { get; set; } = new();
}