namespace OnlineShop.Infrastructure.Payments;

// خطای قابل انتظار در پرداخت آزمایشی
// کنترلر این نوع خطا را به پاسخ ۴۰۹ تبدیل می‌کند
public sealed class SimulatedPaymentConflictException : InvalidOperationException
{
    public SimulatedPaymentConflictException(string message)
        : base(message)
    {
    }
}