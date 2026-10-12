using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Application.DTOs.Cart;

public class UpdateCartItemQuantityDto
{
    // تعداد جدیدی که کاربر برای محصول انتخاب می‌کند
    [Range(1, 100,
        ErrorMessage = "تعداد محصول باید بین ۱ تا ۱۰۰ باشد.")]
    public int Quantity { get; set; }
}