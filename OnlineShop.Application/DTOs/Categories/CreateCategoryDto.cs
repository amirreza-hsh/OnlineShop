using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Application.DTOs.Categories;

public class CreateCategoryDto
{
    // [Required]: این فیلد اجباری است. اگر خالی باشد، فریم‌ورک خودش 400 برمی‌گرداند
    // و اصلاً اجازه نمی‌دهد درخواست به سرویس برسد!
    [Required(ErrorMessage = "نام دسته‌بندی الزامی است")]

    // [StringLength]: جلوی داده‌های غول‌پیکر رو می‌گیرد؛ اسم دسته بیشتر از 100 حرف بی‌معنی است!
    [StringLength(100, ErrorMessage = "نام دسته‌بندی حداکثر 100 کاراکتر است")]
    public string Name { get; set; } = string.Empty;

    // توضیحات اختیاری است، ولی اگر کاربر نوشت، نباید از 500 کاراکتر بیشتر باشد
    [StringLength(500, ErrorMessage = "توضیحات حداکثر 500 کاراکتر است")]
    public string? Description { get; set; }
}
