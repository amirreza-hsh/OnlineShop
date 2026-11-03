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
    // دریافت یک صفحه از سفارش‌های متعلق به کاربر
    // شماره صفحه و تعداد سفارش‌های هر صفحه مشخص می‌شوند
    Task<List<OrderSummaryDto>> GetMyOrdersAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    // true یعنی سفارش لغو شده یا قبلاً لغو بوده است
    // false یعنی وضعیت فعلی اجازه لغو نمی‌دهد
    // سفارش ناموجود یا متعلق به دیگران با خطای یافت‌نشدن مشخص می‌شود

    // دریافت فهرست سفارش‌ها برای بخش مدیریت
    // این عملیات باید فقط از مسیر مخصوص مدیر فراخوانی شود
    Task<List<OrderSummaryDto>> GetOrdersForAdminAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    // دریافت جزئیات یک سفارش برای مدیر
    // کنترل مجوز مدیر در کنترلر انجام می‌شود
    Task<OrderDto?> GetOrderForAdminAsync(
        int orderId,
        CancellationToken cancellationToken = default);
}








