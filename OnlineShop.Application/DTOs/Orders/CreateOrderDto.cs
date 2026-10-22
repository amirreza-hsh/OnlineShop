using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Application.DTOs.Orders;

// اطلاعاتی که مشتری برای ساخت سفارش ارسال می‌کند
// اقلام سفارش از سبد حساب واردشده خوانده می‌شوند
public class CreateOrderDto
{
    // گیرنده می‌تواند شخصی متفاوت از صاحب حساب باشد
    // بنابراین نام گیرنده را جداگانه دریافت می‌کنیم
    [Required(ErrorMessage = "نام گیرنده الزامی است")]
    [StringLength(
        100,
        ErrorMessage = "نام گیرنده حداکثر ۱۰۰ کاراکتر است")]
    public string RecipientName { get; set; } = string.Empty;

    // شماره تماس برای هماهنگی تحویل سفارش استفاده می‌شود
    // Phone فقط یک بررسی اولیه قالب است و مالکیت شماره را تأیید نمی‌کند
    [Required(ErrorMessage = "شماره تماس الزامی است")]
    [Phone(ErrorMessage = "قالب شماره تماس معتبر نیست")]
    [StringLength(
        20,
        ErrorMessage = "شماره تماس حداکثر ۲۰ کاراکتر است")]
    public string PhoneNumber { get; set; } = string.Empty;

    // محدودیت طول با ستون مربوط به استان در دیتابیس هماهنگ است
    [Required(ErrorMessage = "استان الزامی است")]
    [StringLength(
        100,
        ErrorMessage = "نام استان حداکثر ۱۰۰ کاراکتر است")]
    public string Province { get; set; } = string.Empty;

    [Required(ErrorMessage = "شهر الزامی است")]
    [StringLength(
        100,
        ErrorMessage = "نام شهر حداکثر ۱۰۰ کاراکتر است")]
    public string City { get; set; } = string.Empty;

    // آدرس را در خود سفارش ثبت می‌کنیم
    // تغییر آدرس حساب در آینده نباید سفارش قبلی را تغییر دهد
    [Required(ErrorMessage = "آدرس الزامی است")]
    [StringLength(
        500,
        ErrorMessage = "آدرس حداکثر ۵۰۰ کاراکتر است")]
    public string Address { get; set; } = string.Empty;

    // در نسخه فعلی کد پستی ده‌رقمی با ارقام انگلیسی دریافت می‌کنیم
    // این بررسی فقط قالب است و وجود واقعی آدرس را اثبات نمی‌کند
    [Required(ErrorMessage = "کد پستی الزامی است")]
    [RegularExpression(
        @"^[0-9]{10}$",
        ErrorMessage = "کد پستی باید دقیقاً ۱۰ رقم انگلیسی باشد")]
    public string PostalCode { get; set; } = string.Empty;
}
