namespace OnlineShop.Domain.Entities;

// رکورد تأیید موفق در جریان پرداخت آزمایشی
public class SimulatedPayment
{
    // شماره سفارش کلید اصلی این جدول هم هست
    // بنابراین هر سفارش فقط یک رکورد پرداخت آزمایشی موفق دارد
    public int OrderId { get; set; }

    // شماره پیگیری داخلی که سرور تولید می‌کند
    public Guid Reference { get; set; }

    // مبلغ از سفارش ذخیره‌شده خوانده می‌شود
    public decimal Amount { get; set; }

    // زمان ثبت تأیید به وقت UTC
    public DateTime ConfirmedAtUtc { get; set; }
}