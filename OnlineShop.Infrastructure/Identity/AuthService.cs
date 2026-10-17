using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Auth;
using OnlineShop.Application.Interfaces;
using OnlineShop.Appp.Dto.Auth;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
namespace OnlineShop.Infrastructure.Identity;

using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;


public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
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
    public async Task<bool> LoginAsync(LoginDto dto)
    {
        // بررسی ورودی‌های خالی
        if (string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return false;
        }

        // دریافت حساب با ایمیل
        var user = await _userManager.FindByEmailAsync(
            dto.Email.Trim());

        if (user == null)
            return false;

        // بررسی رمز و انجام ورود در صورت موفقیت
        var result = await _signInManager.PasswordSignInAsync(
            user,
            dto.Password,
            isPersistent: false,
            lockoutOnFailure: true);

        return result.Succeeded;
    }

}