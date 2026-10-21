using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Application.Interfaces;
namespace OnlineShop.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

// فعال‌سازی رفتارهای استاندارد کنترلر API
[ApiController]

// آدرس پایه: /api/cart
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    // دریافت سرویس سبد خرید از سیستم تزریق وابستگی
    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }
    // GET: /api/cart
    // دریافت سبد متعلق به حسابی که اکنون وارد شده است.
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<CartDto>> GetMyCart()
    {
        // شناسه را از هویت تأییدشده درخواست می‌گیریم.
        // مشتری نمی‌تواند با ارسال شناسه دلخواه، سبد دیگری را انتخاب کند.
        string? userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        // بررسی دفاعی: هویت باید شناسه کاربر داشته باشد.
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "هویت کاربر قابل تشخیص نیست.");
        }

        // همان سرویس قبلی را با شناسه واقعی حساب فراخوانی می‌کنیم.
        // Repository فقط سبد فعال متعلق به این شناسه را جست‌وجو می‌کند.
        var cart = await _cartService.GetCartByUserIdAsync(userId);

        // اگر سبدی وجود نداشته باشد، سرویس فعلی یک DTO خالی برمی‌گرداند.
        // این درخواست به‌تنهایی سبد جدیدی در دیتابیس ذخیره نمی‌کند.
        return Ok(cart);
    }
    // POST: /api/cart/items
    // افزودن محصول به سبد حسابی که اکنون وارد شده است.
    [Authorize]
    [HttpPost("items")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMyCartItem(
        [FromBody] AddToCartDto dto,
        [FromHeader(Name = "X-CSRF-TOKEN")] string? csrfToken)
    {
        // شناسه حساب از هویت تأییدشده درخواست خوانده می‌شود.
        // مشتری نباید بتواند شناسه صاحب سبد را خودش تعیین کند.
        string? userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        // بررسی دفاعی برای هویتی که شناسه کاربر ندارد.
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "هویت کاربر قابل تشخیص نیست.");
        }

        // همان منطق قبلی افزودن محصول را استفاده می‌کنیم:
        // بررسی محصول، فعال‌بودن، موجودی و سقف تعداد.
        // تفاوت این است که شناسه واقعی حساب را می‌فرستیم.
        await _cartService.AddToCartAsync(userId, dto);

        // عملیات موفق است؛ بدنه پاسخ لازم نداریم.
        // برای مشاهده نتیجه، مسیر GET /api/cart را اجرا می‌کنیم.
        return NoContent();
    }
    // PUT: /api/cart/items/4
    // تعیین تعداد نهایی یک محصول در سبد حساب واردشده.
    [Authorize]
    [HttpPut("items/{productId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateMyCartItemQuantity(
        int productId,
        [FromBody] UpdateCartItemQuantityDto dto,
        [FromHeader(Name = "X-CSRF-TOKEN")] string? csrfToken)
    {
        // شناسه صاحب سبد را از هویت معتبر درخواست می‌گیریم.
        // این شناسه در بدنه یا آدرس درخواست از مشتری دریافت نمی‌شود.
        string? userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        // بررسی دفاعی: هویت باید شناسه کاربر داشته باشد.
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "هویت کاربر قابل تشخیص نیست.");
        }

        // productId از آدرس و تعداد جدید از بدنه JSON خوانده می‌شود.
        // سرویس، محصول را فقط داخل سبد همین کاربر جست‌وجو می‌کند
        // و تعداد، فعال‌بودن محصول و موجودی را بررسی می‌کند.
        await _cartService.UpdateItemQuantityAsync(
            userId,
            productId,
            dto);

        // تغییر با موفقیت ذخیره شده؛ پاسخ بدنه ندارد.
        return NoContent();
    }
    // DELETE: /api/cart/items/4
    // حذف کامل یک محصول از سبد حساب واردشده.
    [Authorize]
    [HttpDelete("items/{productId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMyCartItem(
        int productId,
        [FromHeader(Name = "X-CSRF-TOKEN")] string? csrfToken)
    {
        // شناسه صاحب سبد از هویت معتبر درخواست خوانده می‌شود.
        // مشتری نمی‌تواند شناسه کاربر دیگری را به این متد بدهد.
        string? userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        // بررسی دفاعی برای حالتی که هویت، شناسه کاربر ندارد.
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "هویت کاربر قابل تشخیص نیست.");
        }

        // سرویس، محصول را فقط در سبد همین کاربر جست‌وجو می‌کند.
        // productId شناسه محصول است، نه شناسه ردیف سبد.
        // اگر سبد یا ردیف پیدا نشود، خطا به Handler می‌رسد.
        await _cartService.RemoveFromCartAsync(
            userId,
            productId);

        // حذف موفق بوده؛ بدنه پاسخ لازم نداریم.
        return NoContent();
    }
    // DELETE: /api/cart/items
    // حذف تمام ردیف‌های سبد حساب واردشده.
    [Authorize]
    [HttpDelete("items")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearMyCart(
        [FromHeader(Name = "X-CSRF-TOKEN")] string? csrfToken)
    {
        // صاحب سبد از هویت تأییدشده درخواست مشخص می‌شود.
        // بنابراین شناسه کاربر را از مشتری دریافت نمی‌کنیم.
        string? userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        // بررسی دفاعی برای هویتی که شناسه لازم را ندارد.
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "هویت کاربر قابل تشخیص نیست.");
        }

        // سرویس فقط ردیف‌های سبد فعال همین حساب را حذف می‌کند.
        // منطق حذف و ذخیره تغییرات از قبل در سرویس پیاده شده است.
        await _cartService.ClearCartAsync(userId);

        // عملیات موفق است و بدنه پاسخ لازم نداریم.
        return NoContent();
    }
}
