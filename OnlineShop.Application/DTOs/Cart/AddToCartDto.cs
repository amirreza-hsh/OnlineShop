using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Application.DTOs.Cart;

// اطلاعاتی که برای افزودن محصول از کاربر دریافت می‌کنیم
public class AddToCartDto
{
    // شناسه محصول باید عددی مثبت باشد
    [Range(1, int.MaxValue,
        ErrorMessage = "شناسه محصول باید بیشتر از صفر باشد.")]
    public int ProductId { get; set; }

    // تعداد مجاز در هر درخواست
    [Range(1, 100,
        ErrorMessage = "تعداد باید بین ۱ تا ۱۰۰ باشد.")]
    public int Quantity { get; set; } = 1;
}