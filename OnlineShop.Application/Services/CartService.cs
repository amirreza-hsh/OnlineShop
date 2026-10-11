// File: OnlineShop.Application/Services/CartService.cs
using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Application.Interfaces;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Services;

// پیاده‌سازی منطق بیزینس سبد خرید با استفاده از ریپازیتوری 🛒
public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;

    // تزریق وابستگی اینترفیس ریپازیتوری به جای DbContext مستقیم
    public CartService(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    // دریافت سبد خرید کاربر به همراه تبدیل به DTO
    public async Task<CartDto> GetCartByUserIdAsync(string userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);

        // اگر کاربر هنوز سبد خریدی نداشت، یک سبد خالی با شناسه خودش برمی‌گردانیم
        if (cart == null)
        {
            return new CartDto { UserId = userId };
        }

        // مپ کردن موجودیت‌های دیتابیس به DTO خروجی
        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items.Select(item => new CartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList()
        };
    }

    // افزودن کالا به سبد خرید
    public async Task AddToCartAsync(string userId, AddToCartDto dto)
    {
        // ۱. بررسی یا ایجاد سبد خرید برای کاربر
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            await _cartRepository.AddAsync(cart);
            await _cartRepository.SaveChangesAsync();
        }

        // ۲. اعتبارسنجی وجود کالا و دریافت قیمت لحظه‌ای آن
        var product = await _cartRepository.GetProductByIdAsync(dto.ProductId);
        if (product == null)
        {
            throw new Exception("محصول مورد نظر یافت نشد.");
        }

        // ۳. بررسی تکراری بودن آیتم در سبد
        var cartItem = cart.Items.FirstOrDefault(ci => ci.ProductId == dto.ProductId);

        if (cartItem == null)
        {
            // آیتم جدید است و باید اضافه شود
            var newItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                UnitPrice = product.Price
            };
            await _cartRepository.AddItemAsync(newItem);
        }
        else
        {
            // آیتم از قبل موجود است؛ صرفاً تعداد آن را افزایش می‌دهیم
            cartItem.Quantity += dto.Quantity;
        }

        // ۴. ذخیره نهایی تغییرات
        await _cartRepository.SaveChangesAsync();
    }

    // حذف کالا از سبد خرید
    public async Task RemoveFromCartAsync(string userId, int productId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null) return;

        var cartItem = cart.Items.FirstOrDefault(ci => ci.ProductId == productId);
        if (cartItem != null)
        {
            await _cartRepository.RemoveItemAsync(cartItem);
            await _cartRepository.SaveChangesAsync();
        }
    }
}
