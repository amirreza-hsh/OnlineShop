// File: OnlineShop.Application/DTOs/Cart/AddToCartDto.cs
using System.ComponentModel.DataAnnotations;

// دیتایی که کاربر از سمت فرانت‌اِند برای اضافه کردن یک جنس می‌فرستد ➕
public class AddToCartDto
{
    // شناسه محصولی که کاربر روی دکمه "خرید" آن زده است
    [Required]
    public int ProductId { get; set; }

    // تعدادی که کاربر انتخاب کرده (حداقل باید ۱ عدد باشد)
    [Range(1, 100, ErrorMessage = "تعداد باید حداقل ۱ و حداکثر ۱۰۰ عدد باشد.")]
    public int Quantity { get; set; } = 1;
}
