namespace OnlineShop.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }

    // مشخص می‌کند این ردیف متعلق به کدام سفارش است
    public int OrderId { get; set; }

    // ارتباط ردیف با محصول اصلی فروشگاه
    public int ProductId { get; set; }

    // نام محصول در زمان ثبت سفارش
    // برای نمایش سفارش قبلی فقط به نام فعلی محصول تکیه نمی‌کنیم
    public string ProductName { get; set; } = string.Empty;

    // قیمت واحد محصول هنگام ثبت سفارش
    // تخفیف معتبر همان زمان باید در این مبلغ لحاظ شود
    public decimal UnitPrice { get; set; }

    // تعداد خریداری‌شده در این ردیف
    public int Quantity { get; set; }

    // دسترسی به سفارش مرتبط در EF Core
    public Order Order { get; set; } = null!;

    // دسترسی به محصول مرتبط در EF Core
    public Product Product { get; set; } = null!;
}