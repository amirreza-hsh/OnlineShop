using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Products;
using OnlineShop.Application.Interfaces;
using OnlineShop.Application.Security;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        // سرویس از طریق تزریق وابستگی دریافت می‌شود
        _productService = productService;
    }

    // مشاهده فهرست محصولات برای مهمان هم مجاز است
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll()
    {
        var products = await _productService.GetAllProductsAsync();

        return Ok(products);
    }

    // مشاهده جزئیات محصول به ورود نیاز ندارد
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: $"محصولی با شناسه {id} یافت نشد");
        }

        return Ok(product);
    }

    // ساخت محصول فقط برای کاربر دارای نقش مدیر مجاز است
    // توکن CSRF هم باید معتبر باشد چون اطلاعات فروشگاه تغییر می‌کند
    [Authorize(Roles = AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductDto dto,
        [FromHeader(Name = "X-CSRF-TOKEN")] string? csrfToken)
    {
        // پارامتر هدر برای نمایش ورودی آن در Swagger هم کاربرد دارد
        // بررسی واقعی توکن توسط ویژگی بالای متد انجام می‌شود
        var createdProduct =
            await _productService.CreateProductAsync(dto);

        // پاسخ شامل محصول جدید و آدرس دریافت آن است
        return CreatedAtAction(
            nameof(GetById),
            new { id = createdProduct.Id },
            createdProduct);
    }

    // حذف خود محصول فروشگاه فقط برای مدیر مجاز است
    // این عملیات با حذف یک ردیف از سبد مشتری متفاوت است
    [Authorize(Roles = AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        int id,
        [FromHeader(Name = "X-CSRF-TOKEN")] string? csrfToken)
    {
        var isDeleted = await _productService.DeleteProductAsync(id);

        // مقدار false در سرویس فعلی یعنی محصول پیدا نشده است
        if (!isDeleted)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: $"محصولی با شناسه {id} جهت حذف یافت نشد");
        }

        return NoContent();
    }

    // تغییر قیمت و موجودی و مشخصات محصول فقط برای مدیر است
    [Authorize(Roles = AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateProductDto dto,
        [FromHeader(Name = "X-CSRF-TOKEN")] string? csrfToken)
    {
        var isUpdated = await _productService.UpdateProductAsync(
            id,
            dto);

        if (!isUpdated)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: $"محصولی با شناسه {id} جهت ویرایش یافت نشد");
        }

        return NoContent();
    }

    // مشتری بدون ورود هم می‌تواند محصولات یک دسته را مشاهده کند
    [AllowAnonymous]
    [HttpGet("by-category/{categoryId:int}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>>
        GetProductsByCategory(
            int categoryId,
            CancellationToken ct)
    {
        var products = await _productService
            .GetProductsByCategoryIdAsync(categoryId, ct);

        return Ok(products);
    }
}