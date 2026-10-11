namespace OnlineShop.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // مثلاً: کیبورد مکانیکی، نورپردازی، دسک‌پد
    public string? Description { get; set; }

    // رابطه: هر دسته می‌تواند چند محصول داشته باشد
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
