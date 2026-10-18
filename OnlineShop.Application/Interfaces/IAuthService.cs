using OnlineShop.Application.DTOs.Auth;
using OnlineShop.Appp.Dto.Auth;

namespace OnlineShop.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterResultDto> RegisterAsync(RegisterDto dto);

    Task<bool> LoginAsync (LoginDto dto);
    // پایان‌دادن به نشست ورود مرورگر فعلی
    Task LogoutAsync();
}