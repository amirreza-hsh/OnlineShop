using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Application.DTOs.Products;

public class UpdateProductDto
{
    // ۱. عنوان محصول در زمان ویرایش هم الزامی است
    [Required(ErrorMessage = "عنوان محصول الزامی است")]
    [StringLength(200, ErrorMessage = "عنوان محصول حداکثر ۲۰۰ کاراکتر است")]
    public string Title { get; set; } = string.Empty;

    // ۲. قیمت محصول نمی‌تواند صفر یا منفی باشد
    [Range(0.01, double.MaxValue, ErrorMessage = "قیمت محصول باید بیشتر از صفر باشد")]
    public decimal Price { get; set; }

    // ۳. قیمت با تخفیف اختیاری است ولی نباید منفی باشد
    [Range(0, double.MaxValue, ErrorMessage = "قیمت با تخفیف نمی‌تواند منفی باشد")]
    public decimal? DiscountPrice { get; set; }

    // ۴. موجودی انبار حداقل صفر است
    [Range(0, int.MaxValue, ErrorMessage = "موجودی انبار نمی‌تواند منفی باشد")]
    public int Stock { get; set; }

    // ۵. نام پلتفرم بازی برای ویرایش هم الزامی است
    [Required(ErrorMessage = "پلتفرم محصول الزامی است")]
    [StringLength(50, ErrorMessage = "نام پلتفرم حداکثر ۵۰ کاراکتر است")]
    public string Platform { get; set; } = string.Empty;

    // ۶. ریجن بازی نباید بیش از حد طولانی باشد
    [StringLength(50, ErrorMessage = "ریجن حداکثر ۵۰ کاراکتر است")]
    public string Region { get; set; } = string.Empty;

    // ۷. نوع نسخه (مثلاً Standard یا Ultimate)
    [StringLength(50, ErrorMessage = "نوع نسخه حداکثر ۵۰ کاراکتر است")]
    public string EditionType { get; set; } = string.Empty;

    // ۸. دسته‌بندی باید معتبر باشد
    [Range(1, int.MaxValue, ErrorMessage = "شناسه دسته‌بندی معتبر نیست")]
    public int CategoryId { get; set; }
}
