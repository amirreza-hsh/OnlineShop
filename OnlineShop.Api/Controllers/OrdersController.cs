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