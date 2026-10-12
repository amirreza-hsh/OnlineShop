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
        // دریافت سبد به همراه آیتم‌ها و محصولات
        var cart = await _cartRepository.GetByUserIdAsync(userId);

        // برای کاربری که سبد ندارد، پاسخ خالی می‌سازیم
        if (cart == null)
        {
            return new CartDto { UserId = userId };
        }

        // تبدیل اطلاعات سبد به پاسخ API
        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items.Select(item => new CartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,

                // قرار دادن عنوان محصول در خروجی
                ProductName = item.Product.Title,

                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList()
        };
    }

    // افزودن کالا به سبد خرید
    public async Task AddToCartAsync(string userId, AddToCartDto dto)
    {
        // بررسی ورودی در سرویس، حتی اگر از خارج API فراخوانی شود
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("شناسه کاربر الزامی است.");

        if (dto.ProductId <= 0)
            throw new ArgumentException("شناسه محصول معتبر نیست.");

        if (dto.Quantity < 1 || dto.Quantity > 100)
            throw new ArgumentException("تعداد باید بین ۱ تا ۱۰۰ باشد.");

        // ابتدا محصول را بررسی می‌کنیم تا سبد بی‌دلیل ساخته نشود
        var product = await _cartRepository.GetProductByIdAsync(dto.ProductId);

        if (product == null)
            throw new KeyNotFoundException("محصول مورد نظر یافت نشد.");

        if (!product.IsActive)
            throw new ArgumentException("این محصول در حال حاضر قابل خرید نیست.");

        var cart = await _cartRepository.GetByUserIdAsync(userId);

        // بررسی اینکه محصول از قبل در سبد وجود دارد یا نه
        var cartItem = cart?.Items
            .FirstOrDefault(item => item.ProductId == dto.ProductId);

        // تعداد نهایی شامل تعداد قبلی و تعداد درخواست جدید است
        long totalQuantity = (long)(cartItem?.Quantity ?? 0) + dto.Quantity;

        if (totalQuantity > product.Stock)
            throw new ArgumentException(
                $"موجودی کافی نیست. موجودی فعلی: {product.Stock} عدد.");

        // در نبود سبد، آن را همراه آیتم جدید آماده می‌کنیم
        if (cart == null)
        {
            cart = new Cart { UserId = userId };

            cart.Items.Add(new CartItem
            {
                ProductId = product.Id,
                Quantity = dto.Quantity,
                UnitPrice = product.DiscountPrice ?? product.Price
            });

            await _cartRepository.AddAsync(cart);
        }
        else if (cartItem == null)
        {
            // محصول جدید را به سبد موجود اضافه می‌کنیم
            await _cartRepository.AddItemAsync(new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                Quantity = dto.Quantity,
                UnitPrice = product.DiscountPrice ?? product.Price
            });
        }
        else
        {
            // برای محصول تکراری، تعداد همان ردیف را افزایش می‌دهیم
            cartItem.Quantity = (int)totalQuantity;
        }

        cart.UpdatedAt = DateTime.UtcNow;

        // تغییرات سبد و آیتم را با یک بار ذخیره ثبت می‌کنیم
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
