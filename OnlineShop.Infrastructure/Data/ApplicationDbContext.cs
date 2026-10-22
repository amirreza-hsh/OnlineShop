// File: OnlineShop.Infrastructure/Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Enums;
using OnlineShop.Infrastructure.Identity;
namespace OnlineShop.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
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
    // جدول سفارش‌های ثبت‌شده
    public DbSet<Order> Orders { get; set; }

    // جدول محصولات داخل سفارش‌ها
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // محدود کردن طول شناسه کاربر برای امکان ساخت Index
        // محدودکردن طول شناسه برای هماهنگی با شناسه کاربران Identity
        modelBuilder.Entity<Order>()
            .Property(order => order.UserId)
            .IsRequired()
            .HasMaxLength(450);

        // رابطه سفارش با حساب کاربری
        // حذف حساب نباید به حذف خودکار سابقه سفارش‌ها منجر شود
        modelBuilder.Entity<Order>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // بیشتر درخواست‌های مشتری، سفارش‌های متعلق به خودش را می‌خوانند
        // این ایندکس برای جست‌وجو بر اساس صاحب سفارش مفید است
        modelBuilder.Entity<Order>()
            .HasIndex(order => order.UserId);

        // دقت مبلغ سفارش با قیمت‌های فعلی پروژه هماهنگ است
        modelBuilder.Entity<Order>()
            .Property(order => order.TotalAmount)
            .HasPrecision(18, 2);

        // وضعیت را به شکل عدد صحیح ذخیره می‌کنیم
        modelBuilder.Entity<Order>()
            .Property(order => order.Status)
            .HasConversion<int>()
            .HasDefaultValue(OrderStatus.PendingPayment);

        // محدودیت طول ستون‌های اطلاعات گیرنده
        modelBuilder.Entity<Order>()
            .Property(order => order.RecipientName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Order>()
            .Property(order => order.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        modelBuilder.Entity<Order>()
            .Property(order => order.Province)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Order>()
            .Property(order => order.City)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Order>()
            .Property(order => order.Address)
            .IsRequired()
            .HasMaxLength(500);

        modelBuilder.Entity<Order>()
            .Property(order => order.PostalCode)
            .IsRequired()
            .HasMaxLength(10);

        // ارتباط یک سفارش با چند ردیف محصول
        // حذف خود سفارش باعث حذف ردیف‌های وابسته می‌شود
        // فعلاً مسیر عمومی برای حذف سفارش ایجاد نمی‌کنیم
        modelBuilder.Entity<Order>()
            .HasMany(order => order.Items)
            .WithOne(item => item.Order)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // محصولی که در سفارش استفاده شده نباید با حذف فیزیکی
        // باعث حذف ردیف سفارش یا ازبین‌رفتن ارتباط آن شود
        modelBuilder.Entity<OrderItem>()
            .HasOne(item => item.Product)
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // نام ثبت‌شده محصول را داخل ردیف سفارش نگه می‌داریم
        modelBuilder.Entity<OrderItem>()
            .Property(item => item.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        // قیمت ردیف سفارش هم مانند سایر قیمت‌ها از نوع decimal است
        modelBuilder.Entity<OrderItem>()
            .Property(item => item.UnitPrice)
            .HasPrecision(18, 2);
        modelBuilder.Entity<Cart>()
            .Property(c => c.UserId)
            .IsRequired()
            .HasMaxLength(450);

        // هر کاربر حداکثر یک سبد فعال داشته باشد
        modelBuilder.Entity<Cart>()
            .HasIndex(c => c.UserId)
            .IsUnique()
            .HasDatabaseName("IX_Carts_UserId_Active")
            .HasFilter("[IsActive] = 1");

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
