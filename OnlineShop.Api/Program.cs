using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Api.ExceptionHandling;
using OnlineShop.Application.Interfaces;
using OnlineShop.Application.Services;
using OnlineShop.Infrastructure.Data;
using OnlineShop.Infrastructure.Identity;
using OnlineShop.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ثبت کنترلرها همراه سرویس‌های موردنیاز
// برای فیلتر داخلی ValidateAntiForgeryToken
builder.Services.AddControllersWithViews();
// مستندسازی API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// اتصال دیتابیس
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// مدیریت کاربران و نقش‌ها
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        // قوانین انتخاب رمز
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;

        // تنظیمات قفل موقت حساب
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 10;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(5);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager();

// احراز هویت با کوکی‌های Identity
builder.Services
    .AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

// تنظیمات کوکی ورود
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "OnlineShop.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;

    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});

// ابزار بررسی مجوز دسترسی
builder.Services.AddAuthorization();

// محصولات
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

// دسته‌بندی‌ها
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// سبد خرید
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();

// حساب کاربری
builder.Services.AddScoped<IAuthService, AuthService>();

// مدیریت متمرکز خطاها
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
// تنظیم توکن محافظت از درخواست
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";

    options.Cookie.Name = "OnlineShop.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});


var app = builder.Build();

// رسیدگی به خطاهای درخواست
app.UseExceptionHandler();

// Swagger در محیط توسعه
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ابتدا شناسایی کاربر، سپس بررسی مجوز
app.UseAuthentication();
app.UseAuthorization();

// اتصال مسیرهای کنترلرها
app.MapControllers();

app.Run();

// دسترسی پروژه تست به نقطه شروع برنامه
public partial class Program
{
}