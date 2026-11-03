using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Orders;
using OnlineShop.Application.Interfaces;
using OnlineShop.Application.Security;

namespace OnlineShop.Api.Controllers;

// ورود به‌تنهایی کافی نیست و کاربر باید نقش مدیر داشته باشد
// این محدودیت روی همه متدهای این کنترلر اعمال می‌شود
[Authorize(Roles = AppRoles.Admin)]
[ApiController]
[Route("api/admin/orders")]
public class AdminOrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    // سیستم DI سرویس سفارش را در اختیار کنترلر می‌گذارد
    public AdminOrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // دریافت یک صفحه از سفارش‌های همه مشتری‌ها
    [HttpGet]
    public async Task<ActionResult<List<OrderSummaryDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        // قبل از رسیدن به این متد مجوز مدیر بررسی شده است
        var orders = await _orderService.GetOrdersForAdminAsync(
            page,
            pageSize,
            cancellationToken);

        // خالی‌بودن فهرست خطا محسوب نمی‌شود
        return Ok(orders);
    }

    // دریافت اطلاعات کامل یک سفارش برای رسیدگی توسط مدیر
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderForAdminAsync(
            id,
            cancellationToken);

        // اگر شماره سفارش وجود نداشته باشد پاسخ ۴۰۴ می‌دهیم
        if (order == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "سفارش یافت نشد");
        }

        // جزئیات شامل گیرنده و محصولات سفارش برگردانده می‌شود
        return Ok(order);
    }
}