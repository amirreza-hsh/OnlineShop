using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OnlineShop.Application.Interfaces;
using Xunit;

namespace OnlineShop.Tests;

public class CartEndpointsTests
{
    // این متد کمکی، برنامه را با تنظیمات مخصوص تست آماده می‌کند.
    // برای جلوگیری از تکرار تنظیمات، تست‌ها از همین متد استفاده می‌کنند.
    private static WebApplicationFactory<Program> CreateFactory(
        MissingCartItemService service)
    {
        // Program نقطه شروع همان پروژه API است.
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // میزبان آزمایشی در محیط Development اجرا می‌شود.
                builder.UseEnvironment("Development");

                // تغییر سرویس‌ها فقط در همین میزبان آزمایشی اعمال می‌شود.
                // فایل Program.cs برنامه اصلی تغییر نمی‌کند.
                builder.ConfigureTestServices(services =>
                {
                    // سرویس واقعی سبد را از ثبت‌های این میزبان حذف می‌کنیم.
                    services.RemoveAll<ICartService>();

                    // همان نمونه سرویس آزمایشی را ثبت می‌کنیم.
                    // تست بعداً می‌تواند LastRemovedUserId آن را بخواند.
                    services.AddSingleton<ICartService>(service);

                    // روش پیش‌فرض شناسایی درخواست در تست را تغییر می‌دهیم.
                    services
                        .AddAuthentication(options =>
                        {
                            // این Handler هویت درخواست را می‌سازد.
                            options.DefaultAuthenticateScheme =
                                TestAuthHandler.SchemeName;

                            // اگر مسیر به ورود نیاز داشت و کاربر هویت نداشت،
                            // پاسخ Challenge توسط همین روش ایجاد می‌شود.
                            options.DefaultChallengeScheme =
                                TestAuthHandler.SchemeName;
                        })
                        .AddScheme<
                            AuthenticationSchemeOptions,
                            TestAuthHandler>(
                                TestAuthHandler.SchemeName,
                                _ => { });
                });
            });
    }

    // ساخت مشتری HTTP برای ارسال درخواست به سرور آزمایشی.
    private static HttpClient CreateClient(
        WebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                // کوکی CSRF در پروژه فقط روی HTTPS ارسال می‌شود.
                BaseAddress = new Uri("https://localhost"),

                // اگر پاسخ Redirect دریافت شد، خودکار دنبال آن نرویم.
                // می‌خواهیم پاسخ اولیه سرور را مستقیماً بررسی کنیم.
                AllowAutoRedirect = false,

                // کوکی دریافتی از مسیر csrf نگهداری شود
                // و در درخواست بعدی همراه توکن ارسال شود.
                HandleCookies = true
            });
    }

    [Fact]
    public async Task GetCart_WithoutLogin_ShouldReturn401()
    {
        // Arrange: آماده‌سازی سرویس و برنامه آزمایشی.
        var service = new MissingCartItemService(
            new InvalidOperationException());

        using var factory = CreateFactory(service);
        using var client = CreateClient(factory);

        // هدر X-Test-User نمی‌فرستیم؛ پس هویتی ایجاد نمی‌شود.
        // ویژگی Authorize باید درخواست را پیش از سرویس رد کند.

        // Act: درخواست سبد از مسیر واقعی.
        using var response = await client.GetAsync("/api/cart");

        // Assert: درخواست بدون ورود باید 401 بگیرد.
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task RemoveItem_WithoutCsrf_ShouldReturn400()
    {
        // Arrange
        var service = new MissingCartItemService(
            new InvalidOperationException());

        using var factory = CreateFactory(service);
        using var client = CreateClient(factory);

        // این هدر در میزبان تست، هویت customer-a را ایجاد می‌کند.
        client.DefaultRequestHeaders.Add(
            "X-Test-User",
            "customer-a");

        // عمداً توکن CSRF نمی‌گیریم و هدر آن را هم نمی‌فرستیم.

        // Act
        using var response = await client.DeleteAsync(
            "/api/cart/items/4");

        // Assert: هویت داریم، اما محافظت CSRF باید درخواست را رد کند.
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        // اگر سرویس اجرا نشده باشد، شناسه‌ای هم در آن ثبت نشده است.
        // بنابراین فقط کد پاسخ را بررسی نمی‌کنیم؛
        // مطمئن می‌شویم عملیات حذف اصلاً فراخوانی نشده.
        Assert.Null(service.LastRemovedUserId);
    }

    // Theory یک متد تست را با ورودی‌های متفاوت اجرا می‌کند.
    // این دو InlineData یعنی دو اجرای جداگانه خواهیم داشت.
    [Theory]
    [InlineData(404)]
    [InlineData(500)]
    public async Task RemoveItem_WithLoginAndCsrf_ShouldHandleServiceError(
        int expectedStatus)
    {
        // متن داخلی فرضی که نباید در پاسخ سرور منتشر شود.
        const string internalMessage = "Sensitive internal error";

        // برای سناریوی 404، خطای محصول ناموجود ایجاد می‌کنیم.
        // برای سناریوی 500، یک خطای غیرمنتظره ایجاد می‌کنیم.
        Exception exception = expectedStatus == 404
            ? new KeyNotFoundException("محصول در سبد خرید یافت نشد.")
            : new InvalidOperationException(internalMessage);

        var service = new MissingCartItemService(exception);

        using var factory = CreateFactory(service);
        using var client = CreateClient(factory);

        // این هدر در تمام درخواست‌های همین client ارسال می‌شود.
        // بنابراین هویت هنگام دریافت توکن و هنگام حذف، یکسان می‌ماند.
        client.DefaultRequestHeaders.Add(
            "X-Test-User",
            "customer-a");

        // دریافت توکن واقعی از مسیر موجود در AuthController.
        // کوکی همراه آن نیز به‌کمک HandleCookies نگهداری می‌شود.
        using var csrfResponse = await client.GetAsync(
            "/api/auth/csrf");

        // اگر دریافت توکن موفق نباشد، تست همین‌جا شکست می‌خورد.
        csrfResponse.EnsureSuccessStatusCode();

        // خواندن پاسخ JSON دریافت توکن.
        string csrfBody = await csrfResponse.Content.ReadAsStringAsync();

        using var csrfJson = JsonDocument.Parse(csrfBody);

        // مقدار requestToken را از بدنه پاسخ جدا می‌کنیم.
        string token = csrfJson.RootElement
            .GetProperty("requestToken")
            .GetString()!;

        // برای افزودن هدر به یک درخواست مشخص،
        // درخواست را به‌صورت HttpRequestMessage می‌سازیم.
        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            "/api/cart/items/4");

        // نام این هدر با تنظیم HeaderName در Program.cs یکسان است.
        request.Headers.Add("X-CSRF-TOKEN", token);

        // Act: درخواست دارای هویت و CSRF معتبر است؛
        // پس باید از فیلترها عبور کند و به سرویس برسد.
        using var response = await client.SendAsync(request);

        // بررسی کد HTTP واقعی پاسخ.
        Assert.Equal(
            expectedStatus,
            (int)response.StatusCode);

        // تبدیل بدنه JSON به مدل استاندارد خطا.
        var problem = await response.Content
            .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);

        // کد داخل بدنه هم باید با کد HTTP هماهنگ باشد.
        Assert.Equal(expectedStatus, problem.Status);

        // کنترلر باید شناسه کاربر درخواست را به سرویس فرستاده باشد،
        // نه مقدار قدیمی test-user.
        Assert.Equal(
            "customer-a",
            service.LastRemovedUserId);

        // متن مورد انتظار بر اساس نوع خطا انتخاب می‌شود.
        string expectedTitle = expectedStatus == 404
            ? "محصول در سبد خرید یافت نشد."
            : "خطای غیرمنتظره‌ای در سرور رخ داد.";

        Assert.Equal(expectedTitle, problem.Title);

        // بررسی کل بدنه برای جلوگیری از انتشار پیام داخلی.
        string body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain(internalMessage, body);
    }
}