// File: OnlineShop.Application/Interfaces/ICartService.cs
using OnlineShop.Application.DTOs.Cart;

namespace OnlineShop.Application.Interfaces;

// اینترفیس سرویس سبد خرید: تعریف عملیات‌های لازم برای مدیریت سبد 🛒
public interface ICartService
{
    // دریافت سبد خرید فعلی کاربر بر اساس شناسه کاربری
    Task<CartDto> GetCartByUserIdAsync(string userId);

    // اضافه کردن یک محصول به سبد خرید
    Task AddToCartAsync(string userId, AddToCartDto addToCartDto);

    // حذف یک محصول از سبد خرید
    Task RemoveFromCartAsync(string userId, int productId);
}
