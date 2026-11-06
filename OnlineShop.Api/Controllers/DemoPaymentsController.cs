using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Infrastructure.Payments;

namespace OnlineShop.Api.Controllers;

// برای تأیید پرداخت آزمایشی باید وارد حساب شده باشیم
[Authorize]
[ApiController]
[Route("api/demo-payments")]
public class DemoPaymentsController : ControllerBase
{
    private readonly SimulatedPaymentService _paymentService;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public DemoPaymentsController(
        SimulatedPaymentService paymentService,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _paymentService = paymentService;
        _environment = environment;
        _configuration = configuration;
    }

    // شماره سفارش از آدرس خوانده می‌شود
    // مبلغ از خود سفارش خوانده می‌شود و بدنه درخواست لازم نیست
    [HttpPost("orders/{orderId:int}/confirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(
        int orderId,
        [FromHeader(Name = "X-CSRF-TOKEN")] string? csrfToken,
        CancellationToken cancellationToken)
    {
        // پرداخت آزمایشی فقط در محیط توسعه و با فعال‌سازی صریح اجرا می‌شود
        if (!_environment.IsDevelopment() ||
            !_configuration.GetValue<bool>("DemoPayments:Enabled"))
        {
            return NotFound();
        }

        // شناسه کاربر از نشست ورود می‌آید
        // سرویس با این شناسه مالکیت سفارش را بررسی می‌کند
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        try
        {
            var payment = await _paymentService.ConfirmAsync(
                userId,
                orderId,
                cancellationToken);

            // تکرار تأیید همان رسید قبلی را برمی‌گرداند
            return Ok(new
            {
                payment.OrderId,
                payment.Reference,
                payment.Amount,
                payment.ConfirmedAtUtc
            });
        }
        catch (SimulatedPaymentConflictException exception)
        {
            // وضعیت سفارش یا موجودی با درخواست تأیید سازگار نیست
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: exception.Message);
        }
    }
}