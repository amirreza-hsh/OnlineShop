using OnlineShop.Application.DTOs.Orders;

namespace OnlineShop.Application.Interfaces;

// قرارداد عملیات سفارش
public interface IOrderService
{
    // ثبت سفارش از سبد خرید کاربر
    Task<OrderDto> CreateOrderAsync(
        string userId,
        CreateOrderDto dto,
        CancellationToken cancellationToken = default);

    // دریافت جزئیات سفارش متعلق به کاربر
    Task<OrderDto?> GetOrderByIdAsync(
        string userId,
        int orderId,
        CancellationToken cancellationToken = default);

    // دریافت فهرست سفارش‌های خود کاربر
    Task<List<OrderSummaryDto>> GetMyOrdersAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    // لغو سفارش پرداخت‌نشده متعلق به کاربر
    Task<bool> CancelOrderAsync(
        string userId,
        int orderId,
        CancellationToken cancellationToken = default);

    // دریافت فهرست سفارش‌ها برای مدیر
    Task<List<OrderSummaryDto>> GetOrdersForAdminAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    // دریافت جزئیات یک سفارش برای مدیر
    Task<OrderDto?> GetOrderForAdminAsync(
        int orderId,
        CancellationToken cancellationToken = default);
}