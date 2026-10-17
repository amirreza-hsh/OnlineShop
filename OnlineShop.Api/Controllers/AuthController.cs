using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Auth;
using OnlineShop.Application.Interfaces;
using Microsoft.AspNetCore.Antiforgery;
using OnlineShop.Appp.Dto.Auth;
using System.Reflection;

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
}