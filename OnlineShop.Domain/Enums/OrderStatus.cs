namespace OnlineShop.Domain.Enums;

public enum OrderStatus
{
    // سفارش ساخته شده ولی پرداخت آن تأیید نشده
    PendingPayment = 0,

    // پرداخت توسط سرور تأیید شده
    Paid = 1,

    // سفارش در حال آماده‌سازی است
    Processing = 2,

    // سفارش ارسال شده
    Shipped = 3,

    // سفارش تحویل داده شده
    Delivered = 4,

    // سفارش لغو شده
    Cancelled = 5,

    // مهلت پرداخت سفارش تمام شده
    Expired = 6
}