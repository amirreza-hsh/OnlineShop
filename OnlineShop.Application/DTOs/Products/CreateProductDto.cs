using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Application.DTOs.Products;

public class CreateProductDto
{
    // ۱. عنوان محصول الزامی است و نباید بیش از ۲۰۰ کاراکتر باشد
    [Required(ErrorMessage = "عنوان محصول الزامی است")]
    [StringLength(200, ErrorMessage = "عنوان محصول حداکثر ۲۰۰ کاراکتر است")]
    public string Title { get; set; } = string.Empty;

    // ۲. قیمت باید بزرگتر از صفر باشد؛ از Range استفاده می‌کنیم تا جلوی عدد منفی یا صفر رو بگیره
    [Range(0.01, double.MaxValue, ErrorMessage = "قیمت محصول باید بیشتر از صفر باشد")]
    public decimal Price { get; set; }

    // ۳. قیمت با تخفیف اختیاری است؛ اما اگر وارد شد نباید منفی باشد
    [Range(0, double.MaxValue, ErrorMessage = "قیمت با تخفیف نمی‌تواند منفی باشد")]
    public decimal? DiscountPrice { get; set; }

    // ۴. موجودی انبار نمی‌تواند منفی باشد (حداقل صفر است)
    [Range(0, int.MaxValue, ErrorMessage = "موجودی انبار نمی‌تواند منفی باشد")]
    public int Stock { get; set; }

    // ۵. شناسه دسته‌بندی باید حتماً فرستاده شود و بزرگتر از صفر باشد
    [Range(1, int.MaxValue, ErrorMessage = "شناسه دسته‌بندی معتبر نیست")]
    public int CategoryId { get; set; }

    // ۶. پلتفرم (مثلاً PC, PS5, Xbox) الزامی است
    [Required(ErrorMessage = "پلتفرم محصول الزامی است")]
    [StringLength(50, ErrorMessage = "نام پلتفرم حداکثر ۵۰ کاراکتر است")]
    public string Platform { get; set; } = string.Empty;

    // ۷. ریجن بازی (مثلاً Global, Turkey, US) اختیاری یا کوتاه باشد
    [StringLength(50, ErrorMessage = "ریجن حداکثر ۵۰ کاراکتر است")]
    public string Region { get; set; } = string.Empty;

    // ۸. نوع نسخه (مثلاً Standard, Deluxe, Ultimate)
    [StringLength(50, ErrorMessage = "نوع نسخه حداکثر ۵۰ کاراکتر است")]
    public string EditionType { get; set; } = string.Empty;
}
