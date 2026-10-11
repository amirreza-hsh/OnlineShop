using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Application.DTOs.Categories;

public class UpdateCategoryDto
{
    // فیلد نام برای ویرایش هم اجباری است؛ کاربر نمی‌تواند نام دسته‌بندی را با متن خالی آپدیت کند!
    [Required(ErrorMessage = "نام دسته‌بندی الزامی است")]
    [StringLength(100, ErrorMessage = "نام دسته‌بندی حداکثر می‌تواند 100 کاراکتر باشد")]
    public string Name { get; set; } = string.Empty;

    // توضیحات اختیاری است (علامت ? دارد)، اما اگر مقداری وارد شد نباید بیش از حد طولانی باشد
    [StringLength(500, ErrorMessage = "توضیحات حداکثر می‌تواند 500 کاراکتر باشد")]
    public string? Description { get; set; }
}
