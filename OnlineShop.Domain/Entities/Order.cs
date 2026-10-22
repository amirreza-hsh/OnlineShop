using OnlineShop.Domain.Enums;

namespace OnlineShop.Domain.Entities;

public class Order
{
    public int Id { get; set; }

    // شناسه درخواست ساخت این سفارش
    // برای تشخیص تکرار درخواست نگهداری می‌شود
    public Guid RequestId { get; set; }
    // شناسه حساب صاحب سفارش
    // مقدار آن باید از هویت کاربر واردشده گرفته شود
    public string UserId { get; set; } = string.Empty;

    // زمان ساخت سفارش را با زمان UTC نگه می‌داریم
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // حساب پرداخت‌شده را هنگام ساخت سفارش فرض نمی‌کنیم
    public OrderStatus Status { get; set; } =
        OrderStatus.PendingPayment;

    // مبلغ نهایی که سرور هنگام ساخت سفارش محاسبه می‌کند
    // این مبلغ را مستقیماً از ورودی مشتری قبول نمی‌کنیم
    public decimal TotalAmount { get; set; }

    // اطلاعات گیرنده را داخل خود سفارش ثبت می‌کنیم
    // تغییر مشخصات حساب نباید آدرس سفارش قبلی را تغییر دهد
    public string RecipientName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Province { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    // هر سفارش می‌تواند چند ردیف محصول داشته باشد
    public ICollection<OrderItem> Items { get; set; } =
        new List<OrderItem>();
}