using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Application.Interfaces;

namespace OnlineShop.Api.Controllers;

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

    // این مسیر فعلاً فقط برای آزمایش در محیط توسعه است
    [HttpGet("test")]
    public async Task<ActionResult<CartDto>> GetTestCart(
        [FromServices] IWebHostEnvironment environment)
    {
        // مسیر آزمایشی در محیط انتشار قابل استفاده نیست
        if (!environment.IsDevelopment())
            return NotFound();

        // تا زمان پیاده‌سازی ورود، از یک کاربر آزمایشی استفاده می‌کنیم
        var cart = await _cartService.GetCartByUserIdAsync("test-user");

        // پاسخ موفق با کد 200 و اطلاعات سبد خرید
        return Ok(cart);
    }
    // افزودن محصول به سبد کاربر آزمایشی
    [HttpPost("test/items")]
    public async Task<IActionResult> AddTestItem(
        [FromBody] AddToCartDto dto,
        [FromServices] IWebHostEnvironment environment)
    {
        // این مسیر فقط در محیط توسعه قابل استفاده است
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        // اجرای عملیات افزودن محصول به سبد خرید
        // اگر سرویس خطا بدهد، مدیریت آن به GlobalExceptionHandler می‌رسد
        await _cartService.AddToCartAsync("test-user", dto);

        // اگر عملیات موفق باشد، پاسخ 204 بدون بدنه برمی‌گردانیم
        return NoContent();
    }
    // PUT: api/cart/test/items/4
    // تغییر تعداد یک محصول موجود در سبد کاربر آزمایشی
    [HttpPut("test/items/{productId:int}")]
    public async Task<IActionResult> UpdateTestItemQuantity(
        int productId,
        [FromBody] UpdateCartItemQuantityDto dto,
        [FromServices] IWebHostEnvironment environment)
    {
        // این مسیر فقط در محیط توسعه قابل استفاده است
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        // سرویس، وجود محصول و معتبر بودن تعداد جدید را بررسی می‌کند
        await _cartService.UpdateItemQuantityAsync(
            "test-user",
            productId,
            dto);

        // عملیات موفق بوده و پاسخ بدنه ندارد
        return NoContent();
    }
    // DELETE: api/cart/test/items/4
    // حذف کامل یک محصول از سبد کاربر آزمایشی
    [HttpDelete("test/items/{productId:int}")]
    public async Task<IActionResult> RemoveTestItem(
        int productId,
        [FromServices] IWebHostEnvironment environment)
    {
        // این مسیر فقط در محیط توسعه قابل استفاده است
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        // حذف ردیف محصول از سبد خرید
        await _cartService.RemoveFromCartAsync(
            "test-user",
            productId);

        // حذف موفق بوده و پاسخ بدنه ندارد
        return NoContent();
    }
    // DELETE: api/cart/test/items
    // حذف تمام محصولات از سبد کاربر آزمایشی
    [HttpDelete("test/items")]
    public async Task<IActionResult> ClearTestCart(
        [FromServices] IWebHostEnvironment environment)
    {
        // این مسیر فقط در محیط توسعه قابل استفاده است
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        // اجرای عملیات خالی‌کردن سبد خرید
        await _cartService.ClearCartAsync("test-user");

        // عملیات موفق بوده و پاسخ بدنه ندارد
        return NoContent();
    }
}
