// File: OnlineShop.Application/DTOs/Cart/CartItemDto.cs
namespace OnlineShop.Application.DTOs.Cart;

// DTO برای نمایش جزئیات یک آیتم داخل سبد خرید به کلاینت 📦
public class CartItemDto
{
    // شناسه خود آیتم سبد خرید
    public int Id { get; set; }

    // شناسه محصول خریداری شده
    public int ProductId { get; set; }

    // نام محصول (برای اینکه فرانت‌اند نخواد دوباره درخواست به سرور بزنه)
    public string ProductName { get; set; } = string.Empty;

    // قیمت واحد محصول هنگام اضافه شدن به سبد خرید
    public decimal UnitPrice { get; set; }

    // تعداد درخواستی کاربر
    public int Quantity { get; set; }

    // قیمت کل این آیتم (تعداد ضربدر قیمت واحد)
    public decimal TotalPrice => UnitPrice * Quantity;
}
