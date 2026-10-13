using System.Text;
using Microsoft.AspNetCore.Http;
using OnlineShop.Api.ExceptionHandling;
using Xunit;

namespace OnlineShop.Tests;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task KeyNotFoundException_ShouldReturn404()
    {
        // Arrange: آماده‌سازی محیط آزمایش
        var handler = new GlobalExceptionHandler();

        var context = new DefaultHttpContext();

        // حافظه موقت برای نگهداری پاسخ HTTP
        context.Response.Body = new MemoryStream();

        var exception = new KeyNotFoundException(
            "محصول یافت نشد.");

        // Act: اجرای متدی که قصد آزمایش آن را داریم
        bool handled = await handler.TryHandleAsync(
            context,
            exception,
            CancellationToken.None);

        // Assert: بررسی نتیجه موردانتظار
        Assert.True(handled);

        Assert.Equal(
            StatusCodes.Status404NotFound,
            context.Response.StatusCode);

        // خواندن محتوای پاسخ
        context.Response.Body.Position = 0;

        using var reader = new StreamReader(
            context.Response.Body,
            Encoding.UTF8);

        var response = await reader.ReadToEndAsync();

        // بررسی وجود پیام خطا در پاسخ
        Assert.Contains("محصول یافت نشد", response);
    }
    [Fact]
    public async Task ArgumentException_ShouldReturn400()
    {
        // Arrange: آماده‌سازی درخواست و خطای آزمایشی
        var handler = new GlobalExceptionHandler();
        var context = new DefaultHttpContext();

        context.Response.Body = new MemoryStream();

        var exception = new ArgumentException(
            "تعداد محصول معتبر نیست.");

        // Act: اجرای Handler
        bool handled = await handler.TryHandleAsync(
            context,
            exception,
            CancellationToken.None);

        // Assert: بررسی مدیریت خطا و کد وضعیت
        Assert.True(handled);
        Assert.Equal(400, context.Response.StatusCode);
    }

    [Fact]
    public async Task UnexpectedException_ShouldReturn500()
    {
        // Arrange: ایجاد یک خطای ناشناخته
        var handler = new GlobalExceptionHandler();
        var context = new DefaultHttpContext();

        context.Response.Body = new MemoryStream();

        var exception = new InvalidOperationException(
            "Sensitive internal error");

        // Act: اجرای Handler
        bool handled = await handler.TryHandleAsync(
            context,
            exception,
            CancellationToken.None);

        // Assert: بررسی کد وضعیت
        Assert.True(handled);
        Assert.Equal(500, context.Response.StatusCode);

        // بررسی اینکه اطلاعات حساس در پاسخ منتشر نشده
        context.Response.Body.Position = 0;

        using var reader = new StreamReader(
            context.Response.Body);

        string response = await reader.ReadToEndAsync();

        Assert.DoesNotContain(
            "Sensitive internal error",
            response);
    }
}