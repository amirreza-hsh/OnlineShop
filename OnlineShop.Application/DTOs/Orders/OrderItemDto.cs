namespace OnlineShop.Application.DTOs.Orders;

// اطلاعات یک ردیف سفارش که به مشتری نمایش می‌دهیم
// این کلاس برای پاسخ API است و مستقیماً جدول جدیدی ایجاد نمی‌کند
public class OrderItemDto
{
    // شناسه ردیف سفارش با شناسه محصول متفاوت است
    public int Id { get; set; }

    // شناسه محصول اصلی فروشگاه
    public int ProductId { get; set; }

    // نامی که هنگام ثبت سفارش ذخیره شده است
    // این مقدار لزوماً با نام فعلی محصول یکسان نیست
    public string ProductName { get; set; } = string.Empty;

    // قیمت واحد ثبت‌شده هنگام ساخت سفارش
    public decimal UnitPrice { get; set; }

    // تعداد خریداری‌شده در این ردیف
    public int Quantity { get; set; }

    // مبلغ ردیف از قیمت واحد ضرب‌در تعداد محاسبه می‌شود
    // این ویژگی فقط خواندنی است و ورودی مستقلی ندارد
    public decimal LineTotal => UnitPrice * Quantity;
}