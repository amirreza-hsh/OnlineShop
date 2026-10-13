using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace OnlineShop.Api.ExceptionHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // تعیین کد HTTP براساس نوع خطا
        int statusCode = exception switch
        {
            KeyNotFoundException =>
                StatusCodes.Status404NotFound,

            ArgumentException =>
                StatusCodes.Status400BadRequest,

            DbUpdateException dbEx
                when IsActiveCartConflict(dbEx) =>
                StatusCodes.Status409Conflict,

            _ => StatusCodes.Status500InternalServerError
        };

        // برای خطاهای ناشناخته جزئیات داخلی را منتشر نمی‌کنیم
        string message = statusCode == 500
            ? "خطای غیرمنتظره‌ای در سرور رخ داد."
            : exception.Message;

        // برای تعارض سبد، پیام مشخص برمی‌گردانیم
        if (statusCode == 409)
            message = "یک سبد خرید فعال برای این کاربر وجود دارد.";

        // ساخت پاسخ استاندارد خطا
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = message
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);

        // یعنی خطا توسط این Handler مدیریت شده است
        return true;
    }

    private static bool IsActiveCartConflict(
        DbUpdateException exception)
    {
        // بررسی خطای اصلی SQL Server
        return exception.InnerException is SqlException sqlEx
            && (sqlEx.Number == 2601 || sqlEx.Number == 2627)
            && sqlEx.Message.Contains(
                "IX_Carts_UserId_Active",
                StringComparison.OrdinalIgnoreCase);
    }
}