using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Products;
using OnlineShop.Application.Interfaces;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    // تزریق اینترفیس سرویس به جای دیتابیس (رعایت اصل وارونگی وابستگی DIP)
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // دریافت لیست تمام دیسک‌ها
    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products); // برگرداندن کد 200 به همراه لیست DTOها
    }

    // دریافت یک دیسک خاص بر اساس شناسه
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
    // ایجاد محصول جدید
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductDto dto)
    {
        // اجرای منطق ایجاد محصول در سرویس
        var createdProduct =
            await _productService.CreateProductAsync(dto);

        // پاسخ 201 همراه اطلاعات محصول و آدرس دریافت آن
        return CreatedAtAction(
            nameof(GetById),
            new { id = createdProduct.Id },
            createdProduct);
    }

    // حذف محصول از فروشگاه
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // تلاش برای حذف محصول
        var isDeleted = await _productService.DeleteProductAsync(id);

        // محصولی با این شناسه وجود ندارد
        if (!isDeleted)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: $"محصولی با شناسه {id} جهت حذف یافت نشد");
        }

        // حذف موفق؛ پاسخ بدنه ندارد
        return NoContent();
    }
    // ویرایش اطلاعات یک دیسک بازی
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateProductDto dto)
    {
        var isUpdated = await _productService
            .UpdateProductAsync(id, dto);

        if (!isUpdated)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: $"محصولی با شناسه {id} جهت ویرایش یافت نشد");
        }

        return NoContent();
    }

    // GET: api/Products/by-category/2
    // دریافت محصولات یک دسته‌بندی مشخص بر اساس شناسه دسته‌بندی
    [HttpGet("by-category/{categoryId:int}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int categoryId, CancellationToken ct)
    {
        // صدا زدن سرویس با متدی که در اینترفیس تعریف کردیم
        var products = await _productService.GetProductsByCategoryIdAsync(categoryId, ct);

        // بازگرداندن وضعیت 200 OK به همراه داده‌ها
        return Ok(products);
    }


}
