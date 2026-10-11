// File: OnlineShop.Application/DTOs/Cart/CartDto.cs
namespace OnlineShop.Application.DTOs.Cart;

// DTO برای نمایش وضعیت کلی سبد خرید کاربر 🛒
public class CartDto
{
    // شناسه سبد خرید
    public int Id { get; set; }

    // شناسه کاربر (یا مهمان) صاحب سبد
    public string UserId { get; set; } = string.Empty;

    // لیست تمام محصولات موجود در سبد خرید
    public List<CartItemDto> Items { get; set; } = new();

    // مبلغ نهایی و کل کل سبد خرید که با جمع تمام ردیف‌ها محاسبه می‌شود
    public decimal TotalAmount => Items.Sum(item => item.TotalPrice);
}
