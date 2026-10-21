using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OnlineShop.Tests;

// این کلاس، روش شناسایی کاربر در محیط تست است.
// به‌جای ثبت‌نام و بررسی رمز، از یک هدر آزمایشی استفاده می‌کنیم.
//
// نکته: این Handler فقط در میزبان تست ثبت می‌شود.
// برنامه اصلی همچنان با کوکی Identity کار می‌کند.
public class TestAuthHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    // نام روش احراز هویت آزمایشی.
    // همین نام را هنگام ثبت Handler در تست استفاده می‌کنیم.
    public const string SchemeName = "TestAuth";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        // وابستگی‌های موردنیاز Handler را به کلاس پایه می‌فرستیم.
        // کلاس پایه، امکانات اجرای احراز هویت را فراهم می‌کند.
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // تست می‌تواند شناسه کاربر را در این هدر قرار دهد.
        // مثال: X-Test-User: customer-a
        string userId = Request.Headers["X-Test-User"].ToString();

        // اگر هدر وجود نداشته باشد، هویتی تأیید نمی‌کنیم.
        // در مسیر دارای Authorize، این وضعیت باعث پاسخ 401 می‌شود.
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(
                AuthenticateResult.NoResult());
        }

        // Claim یک داده درباره هویت کاربر است.
        // NameIdentifier را کنترلر سبد برای پیدا کردن صاحب سبد می‌خواند.
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userId)
        };

        // هویت را با نام روش احراز هویت می‌سازیم.
        // مشخص‌بودن authenticationType باعث می‌شود این هویت
        // به‌عنوان هویت احرازشده شناخته شود.
        var identity = new ClaimsIdentity(
            claims,
            SchemeName);

        // ClaimsPrincipal همان شیئی است که بعداً در کنترلر
        // از طریق ویژگی User به آن دسترسی داریم.
        var principal = new ClaimsPrincipal(identity);

        // بلیت احراز هویت، هویت ساخته‌شده و نام روش را نگه می‌دارد.
        var ticket = new AuthenticationTicket(
            principal,
            SchemeName);

        // به سیستم اعلام می‌کنیم احراز هویت آزمایشی موفق بوده است.
        // در این Handler هیچ رمز یا کوکی ورودی بررسی نمی‌شود.
        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}