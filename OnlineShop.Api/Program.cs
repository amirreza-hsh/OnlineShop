using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Api.ExceptionHandling;
using OnlineShop.Application.Interfaces;
using OnlineShop.Application.Services;
using OnlineShop.Infrastructure.Data;
using OnlineShop.Infrastructure.Identity;
using OnlineShop.Infrastructure.Repositories;
using OnlineShop.Application.Security;

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
// این بخش فقط با فعال‌بودن گزینه SeedAdmin اجرا می‌شود
// اجرای معمولی سایت و تست‌ها وارد این بخش نمی‌شود
if (builder.Configuration.GetValue<bool>("SeedAdmin"))
{
    // این فرمان برای آماده‌سازی حساب مدیر در محیط توسعه است
    if (!app.Environment.IsDevelopment())
    {
        throw new InvalidOperationException(
            "فرمان تعیین مدیر فقط در محیط Development مجاز است");
    }

    // ایمیل حساب موردنظر از تنظیمات فرمان خوانده می‌شود
    // این اطلاعات از درخواست ثبت‌نام مشتری دریافت نمی‌شود
    string? adminEmail = builder.Configuration["AdminEmail"];

    if (string.IsNullOrWhiteSpace(adminEmail))
    {
        throw new InvalidOperationException(
            "ایمیل حساب مدیر را با AdminEmail مشخص کنید");
    }

    // ابزارهای مدیریت کاربران و نقش‌ها به Scope نیاز دارند
    // چون اینجا درخواست HTTP نداریم، خودمان Scope می‌سازیم
    using var scope = app.Services.CreateScope();

    // ابزار پیدا کردن حساب و تغییر نقش کاربر
    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<ApplicationUser>>();

    // ابزار ساختن و بررسی نقش‌ها
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    // فقط حساب موجود را مدیر می‌کنیم
    // این فرمان حساب یا رمز پیش‌فرض نمی‌سازد
    var user = await userManager.FindByEmailAsync(
        adminEmail.Trim());

    if (user == null)
    {
        throw new InvalidOperationException(
            "حسابی با این ایمیل پیدا نشد؛ ابتدا ثبت‌نام کنید");
    }

    // نقش Admin را فقط در صورت نبودن ایجاد می‌کنیم
    if (!await roleManager.RoleExistsAsync(AppRoles.Admin))
    {
        var roleResult = await roleManager.CreateAsync(
            new IdentityRole(AppRoles.Admin));

        // ساخت نقش هم ممکن است ناموفق باشد
        // بنابراین نتیجه را قبل از ادامه بررسی می‌کنیم
        if (!roleResult.Succeeded)
        {
            string errors = string.Join(
                " | ",
                roleResult.Errors.Select(error => error.Description));

            throw new InvalidOperationException(errors);
        }
    }

    // قبل از افزودن نقش، عضویت فعلی را بررسی می‌کنیم
    // اجرای دوباره فرمان نباید عضویت تکراری بسازد
    if (!await userManager.IsInRoleAsync(user, AppRoles.Admin))
    {
        var assignmentResult = await userManager.AddToRoleAsync(
            user,
            AppRoles.Admin);

        if (!assignmentResult.Succeeded)
        {
            string errors = string.Join(
                " | ",
                assignmentResult.Errors.Select(
                    error => error.Description));

            throw new InvalidOperationException(errors);
        }
    }

    Console.WriteLine("نقش مدیر برای حساب مشخص‌شده ثبت شد");

    // فرمان تمام شده و در این اجرا سرور را راه‌اندازی نمی‌کنیم
    return;
}

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