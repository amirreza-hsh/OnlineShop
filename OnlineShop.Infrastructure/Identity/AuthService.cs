using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using OnlineShop.Application.DTOs.Auth;
using OnlineShop.Application.Interfaces;

namespace OnlineShop.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<RegisterResultDto> RegisterAsync(RegisterDto dto)
    {
        // بررسی قوانین DTO حتی اگر سرویس خارج از کنترلر فراخوانی شود
        var validationResults = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(
            dto,
            new ValidationContext(dto),
            validationResults,
            validateAllProperties: true);

        if (!isValid)
        {
            string validationMessage = string.Join(
                " | ",
                validationResults.Select(error => error.ErrorMessage));

            throw new ArgumentException(validationMessage);
        }

        // حذف فاصله‌های اضافی ابتدا و انتهای نام و ایمیل
        string email = dto.Email.Trim();
        string fullName = dto.FullName.Trim();

        // آماده‌سازی مدل کاربر
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName
        };

        // بررسی قوانین Identity و ساخت حساب همراه هش رمز
        var result = await _userManager.CreateAsync(
            user,
            dto.Password);

        if (!result.Succeeded)
        {
            string errorMessage = string.Join(
                " | ",
                result.Errors.Select(error => error.Description));

            throw new ArgumentException(errorMessage);
        }

        // خروجی ثبت‌نام بدون اطلاعات رمز
        return new RegisterResultDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = email
        };
    }
}