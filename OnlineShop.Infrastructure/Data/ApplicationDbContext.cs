// File: OnlineShop.Infrastructure/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // جدول محصولات و دیسک‌های بازی در دیتابیس
    public DbSet<Product> Products { get; set; }

    // جدول دسته‌بندی‌ها و ژانرها در دیتابیس
    public DbSet<Category> Categories { get; set; }

    // جدول سبدهای خرید کاربران در دیتابیس 🛒
    public DbSet<Cart> Carts { get; set; }

    // جدول آیتم‌های موجود درون هر سبد خرید 📦
    public DbSet<CartItem> CartItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ۱. تنظیم دقت قیمت برای جلوگیری از خطای ارقام اعشاری در SQL Server
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Product>()
            .Property(p => p.DiscountPrice)
            .HasPrecision(18, 2);

        // ۲. تنظیم دقت اعشار برای قیمت واحد آیتم در سبد خرید
        modelBuilder.Entity<CartItem>()
            .Property(ci => ci.UnitPrice)
            .HasPrecision(18, 2);

        // ۳. تنظیم رابطه بین سبد خرید و آیتم‌ها (اگر سبد حذف شد، تمام آیتم‌های داخلش هم حذف شوند)
        modelBuilder.Entity<Cart>()
            .HasMany(c => c.Items)
            .WithOne(ci => ci.Cart)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // ۴. دیتای پیش‌فرض (Data Seeding) برای دسته‌بندی‌ها تا دیتابیس خالی نمونه
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "PlayStation 5" },
            new Category { Id = 2, Name = "PlayStation 4" },
            new Category { Id = 3, Name = "Xbox Series X/S" },
            new Category { Id = 4, Name = "Nintendo Switch" }
        );
    }
}
