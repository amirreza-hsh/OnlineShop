using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Interfaces;
using OnlineShop.Application.Services;
using OnlineShop.Infrastructure.Data;
using OnlineShop.Infrastructure.Repositories;
using OnlineShop.Api.ExceptionHandling;
using Microsoft.AspNetCore.Identity;
using OnlineShop.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// ۱. اضافه کردن کنترلرها
builder.Services.AddControllers();
// ثبت ریپازیتوری و سرویس در کانتینر DI
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
// برای هر درخواست HTTP، یک نمونه از Repository دسته‌بندی می‌سازد.
// این کلاس کارهای مربوط به خواندن و نوشتن در دیتابیس را انجام می‌دهد.
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// برای هر درخواست HTTP، یک نمونه از سرویس دسته‌بندی می‌سازد.
// این سرویس منطق برنامه را اجرا می‌کند و از Repository کمک می‌گیرد.
builder.Services.AddScoped<ICategoryService, CategoryService>();


// ۲. اضافه کردن سرویس‌های Swagger برای مستندسازی API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ۳. اضافه کردن ارتباط دیتابیس به سیستم
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        // هر ایمیل فقط برای یک حساب استفاده شود
        options.User.RequireUniqueEmail = true;

        // قوانین رمز عبور
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
// ثبت کلاس ارتباط سبد خرید با دیتابیس
builder.Services.AddScoped<ICartRepository, CartRepository>();

// ثبت کلاس اجرای عملیات سبد خرید
builder.Services.AddScoped<ICartService, CartService>();
// ثبت سیستم مدیریت متمرکز خطاها
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// فعال‌سازی پشتیبانی از پاسخ‌های استاندارد ProblemDetails
builder.Services.AddProblemDetails();

builder.Services.AddScoped<IAuthService, AuthService>();
var app = builder.Build();
// فعال‌سازی Middleware مدیریت خطاها
app.UseExceptionHandler();

// ۴. فعال‌سازی محیط Swagger در حالت توسعه (Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// ۵. وصل کردن مسیر کنترلرها
app.MapControllers();

app.Run();
// قابل دسترسی کردن نقطه شروع برنامه برای پروژه تست
public partial class Program
{

}
