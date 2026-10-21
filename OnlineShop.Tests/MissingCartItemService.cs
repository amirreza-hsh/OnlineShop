using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Application.Interfaces;

namespace OnlineShop.Tests;

// این کلاس جایگزین سرویس واقعی سبد در تست می‌شود.
// بنابراین تست‌های این مرحله به اطلاعات دیتابیس وابسته نیستند.
public class MissingCartItemService : ICartService
{
    // خطایی که تست هنگام ساخت سرویس مشخص می‌کند.
    // با یک کلاس می‌توانیم سناریوهای 404 و 500 را آزمایش کنیم.
    private readonly Exception _exception;

    // آخرین شناسه‌ای که کنترلر برای عملیات حذف ارسال کرده است.
    // تست با خواندن این ویژگی بررسی می‌کند صاحب سبد درست انتخاب شده.
    //
    // private set یعنی فقط خود این کلاس می‌تواند مقدار را تغییر دهد.
    public string? LastRemovedUserId { get; private set; }

    public MissingCartItemService(Exception exception)
    {
        _exception = exception;
    }

    public Task RemoveFromCartAsync(string userId, int productId)
    {
        // ثبت شناسه دریافتی برای بررسی در بخش Assert تست.
        LastRemovedUserId = userId;

        // یک Task ناموفق می‌سازیم.
        // وقتی کنترلر آن را await کند، همین Exception دریافت می‌شود.
        return Task.FromException(_exception);
    }

    // چون ICartService را پیاده‌سازی کرده‌ایم،
    // باید تمام متدهای قرارداد را تعریف کنیم.
    //
    // این تست‌ها قرار نیست عملیات زیر را اجرا کنند.
    // اگر اشتباهی فراخوانی شوند، باید آشکارا خطا بدهند؛
    // نه اینکه به‌صورت ساختگی نتیجه موفق برگردانند.

    public Task<CartDto> GetCartByUserIdAsync(string userId)
        => throw new NotSupportedException();

    public Task AddToCartAsync(string userId, AddToCartDto dto)
        => throw new NotSupportedException();

    public Task UpdateItemQuantityAsync(
        string userId,
        int productId,
        UpdateCartItemQuantityDto dto)
        => throw new NotSupportedException();

    public Task ClearCartAsync(string userId)
        => throw new NotSupportedException();
}