namespace OnlineShop.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty; // نام بازی (مثلاً The Last of Us Part I)
    public string Description { get; set; } = string.Empty; // توضیحات بازی و مشخصات پک
    public decimal Price { get; set; } // قیمت دیسک/پک
    public decimal? DiscountPrice { get; set; } // تخفیف اگر داشت
    public int Stock { get; set; } // تعداد دیسک موجود در انبار 📦

    public string Platform { get; set; } = "PS5"; // PS5, PS4, Xbox Series X, PC
    public string EditionType { get; set; } = "Standard"; // نسخه: Standard, Steelbook, Collector's
    public string Region { get; set; } = "All"; // ریجن دیسک: Region 1 (USA), Region 2 (Europe), All

    public string? CoverImageUrl { get; set; } // عکس روی جلد دیسک
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ارتباط با ژانر/دسته‌بندی (Category)
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
