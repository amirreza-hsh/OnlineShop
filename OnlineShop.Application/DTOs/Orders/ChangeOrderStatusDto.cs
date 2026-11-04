using System.ComponentModel.DataAnnotations;
using OnlineShop.Domain.Enums;

namespace OnlineShop.Application.DTOs.Orders;

// اطلاعاتی که مدیر برای تغییر وضعیت می‌فرستد
public class ChangeOrderStatusDto
{
    // nullable کمک می‌کند نفرستادن وضعیت را تشخیص بدهیم
    // Required مقدار null را رد می‌کند
    [Required(ErrorMessage = "وضعیت جدید الزامی است")]

    // فقط عددهای تعریف‌شده در OrderStatus پذیرفته می‌شوند
    [EnumDataType(
        typeof(OrderStatus),
        ErrorMessage = "وضعیت ارسال‌شده معتبر نیست")]
    public OrderStatus? Status { get; set; }
}