using OnlineShop.Application.DTOs.Auth;

namespace OnlineShop.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterResultDto> RegisterAsync(RegisterDto dto);
}