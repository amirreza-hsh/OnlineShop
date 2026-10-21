using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Categories;
using OnlineShop.Application.Interfaces;
using OnlineShop.Application.Security;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    // مشاهده دسته‌بندی‌ها برای همه بازدیدکننده‌ها مجاز است
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll(
        CancellationToken ct)
    {
        var categories = await _service.GetAllCategoriesAsync(ct);

        return Ok(categories);
    }

    // مشاهده یک دسته‌بندی مشخص هم به ورود نیاز ندارد
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(
        int id,
        CancellationToken ct)
    {
        var category = await _service.GetCategoryByIdAsync(id, ct);

        if (category == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: $"دسته‌بندی با شناسه {id} یافت نشد");
        }

        return Ok(category);
    }

    // ایجاد دسته‌بندی به نقش مدیر و توکن CSRF معتبر نیاز دارد
    [Authorize(Roles = AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(
        [FromBody] CreateCategoryDto dto,
        [FromHeader(Name = "X-CSRF-TOKEN")] string? csrfToken,
        CancellationToken ct)
    {
        var created = await _service.CreateCategoryAsync(dto, ct);

        // آدرس دریافت دسته‌بندی ساخته‌شده در پاسخ قرار می‌گیرد
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
    }

    // مدیر اجازه تغییر دسته‌بندی را دارد
    // منطق ویرایش همچنان داخل سرویس باقی مانده است
    [Authorize(Roles = AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCategoryDto dto,
        [FromHeader(Name = "X-CSRF-TOKEN")] string? csrfToken,
        CancellationToken ct)
    {
        bool updated = await _service.UpdateCategoryAsync(
            id,
            dto,
            ct);

        if (!updated)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: $"دسته‌بندی با شناسه {id} یافت نشد");
        }

        return NoContent();
    }

    // محدودیت دسترسی باید در سرور اجرا شود
    // پنهان‌کردن دکمه حذف در ظاهر سایت کافی نیست
    [Authorize(Roles = AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        [FromHeader(Name = "X-CSRF-TOKEN")] string? csrfToken,
        CancellationToken ct)
    {
        bool deleted = await _service.DeleteCategoryAsync(id, ct);

        if (!deleted)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: $"دسته‌بندی با شناسه {id} یافت نشد");
        }

        return NoContent();
    }
}