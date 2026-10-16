using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OnlineShop.Appp.Dto.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "ایمیل الزامی است")]
    [EmailAddress(ErrorMessage = "فرمت ایمیل معتبر نیست")]
    [StringLength(256, ErrorMessage = "ایمیل حداکثر 256 کاراکتر است")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "رمز عبور الزامی است")]
    public string Password { get; set; } = string.Empty;
}