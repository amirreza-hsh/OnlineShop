// File: OnlineShop.Domain/Entities/Cart.cs
namespace OnlineShop.Domain.Entities;

public class Cart
{
    public int Id { get; set; }

    // برای کاربر لاگین‌شده یا کاربر مهمان (Guest / Session Id)
    public string UserId { get; set; } = string.Empty;

    // تاریخ ایجاد و آخرین به‌روزرسانی سبد
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // وضعیت فعال بودن سبد (وقتی پرداخت بشه غیرفعال میشه)
    public bool IsActive { get; set; } = true;

    // لیست آیتم‌های موجود در این سبد خرید
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
