using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Application.DTOs.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "نام کامل الزامی است.")]
    [StringLength(
        100,
        ErrorMessage = "نام کامل حداکثر ۱۰۰ کاراکتر است.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "ایمیل الزامی است.")]
    [EmailAddress(ErrorMessage = "فرمت ایمیل معتبر نیست.")]
    [StringLength(
        256,
        ErrorMessage = "ایمیل حداکثر ۲۵۶ کاراکتر است.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است.")]
    [StringLength(
        128,
        MinimumLength = 8,
        ErrorMessage = "رمز عبور باید بین ۸ تا ۱۲۸ کاراکتر باشد.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "تکرار رمز عبور الزامی است.")]
    [Compare(
        nameof(Password),
        ErrorMessage = "رمز عبور و تکرار آن یکسان نیستند.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}