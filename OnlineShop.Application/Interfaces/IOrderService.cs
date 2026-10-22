using OnlineShop.Application.DTOs.Orders;

namespace OnlineShop.Application.Interfaces;

// قرارداد عملیات سفارش برای استفاده کنترلر
// جزئیات دیتابیس و تراکنش داخل این اینترفیس قرار نمی‌گیرد
public interface IOrderService
{
    // اطلاعات گیرنده از DTO می‌آید
    // شناسه کاربر را کنترلر از هویت تأییدشده درخواست استخراج می‌کند
    // اقلام و قیمت‌ها باید سمت سرور بررسی و تعیین شوند
    Task<OrderDto> CreateOrderAsync(
        string userId,
        CreateOrderDto dto,
        CancellationToken cancellationToken = default);

    // سفارش فقط در صورتی برگردانده می‌شود که متعلق به همین کاربر باشد
    // نبودن سفارش یا تعلق آن به شخص دیگر باید نتیجه قابل مشاهده یکسانی داشته باشد
    Task<OrderDto?> GetOrderByIdAsync(
        string userId,
        int orderId,
        CancellationToken cancellationToken = default);
}