// File: OnlineShop.Domain/Entities/CartItem.cs
namespace OnlineShop.Domain.Entities;

public class CartItem
{
    public int Id { get; set; }

    // شناسه سبد خریدی که این آیتم متعلق به آن است
    public int CartId { get; set; }

    // شناسه محصول انتخاب‌شده
    public int ProductId { get; set; }

    // تعداد درخواستی از این محصول
    public int Quantity { get; set; }

    // قیمتی که در لحظه افزودن به سبد ثبت شده
    public decimal UnitPrice { get; set; }

    // Navigation Properties برای ارتباطات در EF Core
    public Cart Cart { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
