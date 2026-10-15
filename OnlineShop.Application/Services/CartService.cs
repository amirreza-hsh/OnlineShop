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
        // بررسی ورودی‌ها
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("شناسه کاربر الزامی است");

        if (dto.ProductId <= 0)
            throw new ArgumentException("شناسه محصول معتبر نیست");

        if (dto.Quantity < 1 || dto.Quantity > 100)
            throw new ArgumentException("تعداد باید بین ۱ تا ۱۰۰ باشد");

        // بررسی محصول
        var product = await _cartRepository
            .GetProductByIdAsync(dto.ProductId);

        if (product == null)
            throw new KeyNotFoundException("محصول مورد نظر یافت نشد");

        if (!product.IsActive)
            throw new ArgumentException(
                "این محصول در حال حاضر قابل خرید نیست");

        // دریافت سبد و ردیف احتمالی محصول
        var cart = await _cartRepository.GetByUserIdAsync(userId);

        var cartItem = cart?.Items
            .FirstOrDefault(item => item.ProductId == dto.ProductId);

        // محاسبه تعداد نهایی
        long totalQuantity =
            (long)(cartItem?.Quantity ?? 0) + dto.Quantity;

        // بررسی سقف تعداد هر محصول در سبد
        if (totalQuantity > 100)
            throw new ArgumentException(
                "تعداد هر محصول در سبد نمی‌تواند بیشتر از ۱۰۰ باشد");

        // بررسی موجودی برای تعداد نهایی
        if (totalQuantity > product.Stock)
            throw new ArgumentException(
                $"موجودی کافی نیست. موجودی فعلی: {product.Stock} عدد");

        if (cart == null)
        {
            // ساخت سبد همراه اولین محصول
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
            // افزودن محصول جدید به سبد موجود
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
            // افزایش تعداد ردیف موجود
            cartItem.Quantity = (int)totalQuantity;
        }

        cart.UpdatedAt = DateTime.UtcNow;

        await _cartRepository.SaveChangesAsync();
    }
    // حذف کامل یک محصول از سبد خرید کاربر
    public async Task RemoveFromCartAsync(string userId, int productId)
    {
        // بررسی شناسه کاربر
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("شناسه کاربر معتبر نیست");

        // بررسی شناسه محصول
        if (productId <= 0)
            throw new ArgumentException("شناسه محصول معتبر نیست");

        // دریافت سبد خرید کاربر
        var cart = await _cartRepository.GetByUserIdAsync(userId);

        // بررسی وجود سبد فعال
        if (cart == null || !cart.IsActive)
            throw new KeyNotFoundException("سبد خرید فعال یافت نشد");

        // پیدا کردن محصول داخل سبد
        var cartItem = cart.Items
            .FirstOrDefault(item => item.ProductId == productId);

        if (cartItem == null)
            throw new KeyNotFoundException("محصول در سبد خرید یافت نشد");

        // علامت‌گذاری محصول برای حذف
        await _cartRepository.RemoveItemAsync(cartItem);

        // ثبت زمان آخرین تغییر سبد
        cart.UpdatedAt = DateTime.UtcNow;

        // ذخیره واقعی تغییرات در دیتابیس
        await _cartRepository.SaveChangesAsync();
    }

    // تغییر تعداد یک محصول موجود در سبد خرید
    public async Task UpdateItemQuantityAsync(
        string userId,
        int productId,
        UpdateCartItemQuantityDto dto)
    {
        // بررسی معتبر بودن شناسه کاربر
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("شناسه کاربر معتبر نیست");

        // بررسی معتبر بودن شناسه محصول
        if (productId <= 0)
            throw new ArgumentException("شناسه محصول معتبر نیست");

        // اعتبارسنجی تعداد در لایه Application
        if (dto.Quantity < 1 || dto.Quantity > 100)
            throw new ArgumentException("تعداد باید بین ۱ تا ۱۰۰ باشد");

        // دریافت سبد خرید کاربر از دیتابیس
        var cart = await _cartRepository.GetByUserIdAsync(userId);

        // اگر کاربر سبد خرید نداشته باشد
        if (cart == null || !cart.IsActive)
            throw new KeyNotFoundException("سبد خرید فعال یافت نشد");

        // پیدا کردن محصول موردنظر در سبد خرید
        var cartItem = cart.Items
            .FirstOrDefault(item => item.ProductId == productId);

        // محصول باید از قبل در سبد وجود داشته باشد
        if (cartItem == null)
            throw new KeyNotFoundException("محصول در سبد خرید یافت نشد");

        // دریافت آخرین اطلاعات محصول از دیتابیس
        var product = await _cartRepository.GetProductByIdAsync(productId);

        if (product == null)
            throw new KeyNotFoundException("محصول یافت نشد");

        // بررسی فعال بودن محصول
        if (!product.IsActive)
            throw new ArgumentException("محصول در حال حاضر قابل خرید نیست");

        // بررسی موجودی محصول
        if (dto.Quantity > product.Stock)
            throw new ArgumentException(
                $"موجودی کافی نیست. موجودی فعلی: {product.Stock} عدد");

        // تعیین تعداد جدید (نه اضافه کردن به تعداد قبلی)
        cartItem.Quantity = dto.Quantity;

        // ثبت زمان آخرین تغییر سبد خرید
        cart.UpdatedAt = DateTime.UtcNow;

        // ذخیره تغییرات در دیتابیس
        await _cartRepository.SaveChangesAsync();
    }
    // خالی کردن تمام محصولات سبد خرید یک کاربر
    public async Task ClearCartAsync(string userId)
    {
        // بررسی معتبر بودن شناسه کاربر
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("شناسه کاربر معتبر نیست");

        // دریافت سبد خرید کاربر از Repository
        var cart = await _cartRepository.GetByUserIdAsync(userId);

        // بررسی وجود سبد خرید فعال
        if (cart == null || !cart.IsActive)
            throw new KeyNotFoundException("سبد خرید فعال یافت نشد");

        // اگر سبد از قبل خالی باشد، کاری انجام نمی‌دهیم
        if (!cart.Items.Any())
            return;

        // حذف تمام آیتم‌های متعلق به همین سبد
        _cartRepository.RemoveItems(cart.Items.ToList());

        // ثبت آخرین زمان تغییر محتویات سبد
        cart.UpdatedAt = DateTime.UtcNow;

        // ذخیره نهایی تغییرات در دیتابیس
        await _cartRepository.SaveChangesAsync();
    }
}
