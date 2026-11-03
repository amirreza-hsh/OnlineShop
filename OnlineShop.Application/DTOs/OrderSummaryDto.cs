using OnlineShop.Domain.Enums;

namespace OnlineShop.Application.DTOs.Orders;

// اطلاعات مختصر هر سفارش در صفحه فهرست
// جزئیات آدرس و محصولات از مسیر جزئیات سفارش دریافت می‌شوند
public class OrderSummaryDto
{
    // شماره سفارش برای باز کردن صفحه جزئیات
    public int Id { get; set; }

    // زمان ثبت سفارش
    public DateTime CreatedAt { get; set; }

    // وضعیت فعلی سفارش
    public OrderStatus Status { get; set; }

    // نام متنی وضعیت از مقدار Status محاسبه می‌شود
    public string StatusName => Status.ToString();

    // مبلغ ثبت‌شده سفارش
    public decimal TotalAmount { get; set; }
}