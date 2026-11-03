using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Orders;
using OnlineShop.Application.Interfaces;

namespace OnlineShop.Api.Controllers;

// تمام مسیرهای این کنترلر به ورود نیاز دارند
[Authorize]
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // دریافت فهرست سفارش‌های حساب واردشده
    // ویژگی Authorize روی کلاس، ورود را برای این مسیر هم الزامی می‌کند
    [HttpGet]
    public async Task<ActionResult<List<OrderSummaryDto>>> GetMyOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        // شناسه صاحب سفارش‌ها را از هویت تأییدشده می‌گیریم
        // مشتری نمی‌تواند شناسه کاربر دیگری را در ورودی انتخاب کند
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // اگر شناسه کاربر در هویت موجود نباشد درخواست پذیرفته نمی‌شود
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        // خواندن صفحه موردنظر از سفارش‌های همین کاربر
        var orders = await _orderService.GetMyOrdersAsync(
            userId,
            page,
            pageSize,
            cancellationToken);

        // نداشتن سفارش خطا نیست و با فهرست خالی پاسخ داده می‌شود
        return Ok(orders);
    }

    // ساخت سفارش از سبد فعال حساب واردشده
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<OrderDto>> Create(
        [FromBody] CreateOrderDto dto,
        [FromHeader(Name = "X-CSRF-TOKEN")] string? csrfToken,
        CancellationToken cancellationToken)
    {
        // صاحب سفارش از هویت معتبر درخواست تعیین می‌شود
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var order = await _orderService.CreateOrderAsync(
            userId,
            dto,
            cancellationToken);

        // پاسخ همراه آدرس دریافت جزئیات سفارش برمی‌گردد
        return CreatedAtAction(
            nameof(GetById),
            new { id = order.Id },
            order);
    }

    // دریافت جزئیات سفارش متعلق به همین حساب
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var order = await _orderService.GetOrderByIdAsync(
            userId,
            id,
            cancellationToken);

        // سفارش ناموجود و سفارش متعلق به شخص دیگر پاسخ یکسان می‌گیرند
        if (order == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "سفارش یافت نشد");
        }

        return Ok(order);
    }
}
