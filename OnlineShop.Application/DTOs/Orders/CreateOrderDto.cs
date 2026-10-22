using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Application.DTOs.Orders;

public class CreateOrderDto
{
    // شناسه درخواست از نوع Guid است و رشته نیست
    // بنابراین StringLength نباید روی آن قرار بگیرد
    // خالی‌نبودن آن با مقایسه Guid.Empty در سرویس بررسی می‌شود
    public Guid RequestId { get; set; }

    // این دو ویژگی فقط به RecipientName مربوط هستند
    // باید مستقیماً قبل از همان ویژگی قرار بگیرند
    [Required(ErrorMessage = "نام گیرنده الزامی است")]
    [StringLength(
        100,
        ErrorMessage = "نام گیرنده حداکثر ۱۰۰ کاراکتر است")]
    public string RecipientName { get; set; } = string.Empty;

    // شماره تماس یک رشته است و محدودیت طول برای آن معتبر است
    [Required(ErrorMessage = "شماره تماس الزامی است")]
    [Phone(ErrorMessage = "قالب شماره تماس معتبر نیست")]
    [StringLength(
        20,
        ErrorMessage = "شماره تماس حداکثر ۲۰ کاراکتر است")]
    public string PhoneNumber { get; set; } = string.Empty;

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

    [Required(ErrorMessage = "آدرس الزامی است")]
    [StringLength(
        500,
        ErrorMessage = "آدرس حداکثر ۵۰۰ کاراکتر است")]
    public string Address { get; set; } = string.Empty;

    // کد پستی به شکل رشته نگهداری می‌شود
    // تا صفر احتمالی ابتدای آن حفظ شود
    [Required(ErrorMessage = "کد پستی الزامی است")]
    [RegularExpression(
        @"^[0-9]{10}$",
        ErrorMessage = "کد پستی باید دقیقاً ۱۰ رقم انگلیسی باشد")]
    public string PostalCode { get; set; } = string.Empty;
}