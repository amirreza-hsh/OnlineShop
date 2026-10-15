using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Auth;
using OnlineShop.Application.Interfaces;

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

    // ساخت حساب کاربری جدید
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResultDto>> Register(
        [FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }
}