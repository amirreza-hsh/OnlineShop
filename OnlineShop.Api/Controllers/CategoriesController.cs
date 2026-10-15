// کتابخانه‌های لازم برای ساخت Controller و ویژگی‌های Routing
using Microsoft.AspNetCore.Mvc;
// DTO های دسته‌بندی (برای تبادل تمیز اطلاعات)
using OnlineShop.Application.DTOs.Categories;
// قرارداد ICategoryService (کنترلر به اینترفیس وصل می‌شود نه کلاس مستقیم)
using OnlineShop.Application.Interfaces;

namespace OnlineShop.Api.Controllers;

// [ApiController]: ویژگی‌های هوشمند API مثل خطایابی خودکار مدل‌ها
// [Route("api/[controller]")]: مسیر اندپوینت می‌شود: /api/categories
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    // فیلد سرویس
    private readonly ICategoryService _service;

    // تزریق وابستگی (Dependency Injection) سرویس به کنترلر
    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    // GET /api/categories -> دریافت همه‌ی دسته‌ها (200 OK)
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllCategoriesAsync(ct));

    // GET /api/categories/5 -> دریافت یک دسته با شناسه عددی
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id, CancellationToken ct)
    {
        var category = await _service.GetCategoryByIdAsync(id, ct);
        return category is null
            ? NotFound($"دسته‌بندی با شناسه {id} یافت نشد")
            : Ok(category);
    }

    // POST /api/categories -> ایجاد دسته جدید (201 Created)
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryDto dto, CancellationToken ct)
    {
        var created = await _service.CreateCategoryAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT /api/categories/5 -> ویرایش دسته (204 No Content)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCategoryDto dto, CancellationToken ct)
        => await _service.UpdateCategoryAsync(id, dto, ct) ? NoContent() : NotFound();

    // DELETE /api/categories/5 -> حذف دسته (204 No Content)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await _service.DeleteCategoryAsync(id, ct) ? NoContent() : NotFound();
}
