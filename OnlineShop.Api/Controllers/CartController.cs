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
            return NotFound();

        try
        {
            await _cartService.AddToCartAsync("test-user", dto);

            // عملیات موفق بوده و پاسخ بدنه ندارد
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            // محصول پیدا نشده است
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            // ورودی یا شرایط خرید معتبر نیست
            return BadRequest(new { message = ex.Message });
        }
    }
}
