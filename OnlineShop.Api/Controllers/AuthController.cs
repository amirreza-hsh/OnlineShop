using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Auth;
using OnlineShop.Application.Interfaces;
using Microsoft.AspNetCore.Antiforgery;
using OnlineShop.Appp.Dto.Auth;
using System.Reflection;
using System.Security.Claims;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    // دریافت توکن محافظت از درخواست
    [AllowAnonymous]
    [HttpGet("csrf")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult GetCsrfToken(
        [FromServices] IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);

        return Ok(new
        {
            requestToken = tokens.RequestToken
        });
    }

    // ورود به حساب کاربری
    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto,
        [FromHeader(Name = "X-CSRF-TOKEN")] string? csrfToken)
    {
        bool succeeded = await _authService.LoginAsync(dto);

        if (!succeeded)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "ورود انجام نشد. اطلاعات ورود یا وضعیت حساب را بررسی کنید.");
        }

        return NoContent();
    }
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResultDto>> Register(
        [FromBody] RegisterDto dto)
    {
        // اطلاعات درخواست به سرویس ثبت‌نام فرستاده می‌شود.
        // سرویس، اعتبارسنجی و ساخت حساب با Identity را انجام می‌دهد.
        var result = await _authService.RegisterAsync(dto);

        // کد 201 یعنی حساب جدید با موفقیت ساخته شده است.
        // نتیجه شامل شناسه، نام و ایمیل است؛ رمز برگردانده نمی‌شود.
        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }
    // GET: /api/auth/me
    // اطلاعات هویتی کاربری را برمی‌گرداند که اکنون وارد حساب است.
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        // Middleware احراز هویت، کوکی را بررسی کرده
        // و هویت معتبر درخواست را در ویژگی User قرار داده است.
        // شناسه را از این هویت می‌خوانیم، نه از ورودی مشتری.
        string? userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        // این بررسی برای حالتی است که هویت، شناسه لازم را نداشته باشد.
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "هویت کاربر قابل تشخیص نیست.");
        }

        // ایمیل را از Claimهای هویت ورود می‌خوانیم.
        // این متد برای دریافت ایمیل، مستقیماً دیتابیس را جست‌وجو نمی‌کند.
        string? email = User.FindFirstValue(
            ClaimTypes.Email);

        // پاسخ 200 همراه اطلاعات محدود حساب.
        // رمز، هش رمز و مقدار کوکی در پاسخ قرار نمی‌گیرند.
        return Ok(new
        {
            userId,
            email
        });
    }
    // POST: /api/auth/logout
    // خروج از حساب در مرورگری که درخواست را ارسال می‌کند.
    [Authorize]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(
        [FromHeader(Name = "X-CSRF-TOKEN")] string? csrfToken)
    {
        // ویژگی ValidateAntiForgeryToken قبل از اجرای متد،
        // توکن هدر و کوکی همراه آن را اعتبارسنجی می‌کند.
        // پارامتر بالا برای نمایش ورودی هدر در Swagger هم کاربرد دارد.

        // سرویس، عملیات خروج را با ابزار Identity انجام می‌دهد.
        await _authService.LogoutAsync();

        // عملیات موفق است و نیازی به بدنه پاسخ نداریم.
        return NoContent();
    }
}