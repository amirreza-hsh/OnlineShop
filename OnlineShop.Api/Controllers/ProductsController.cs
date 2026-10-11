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
            return NotFound($"محصولی با شناسه {id} در سیستم یافت نشد.");

        return Ok(product);
    }

    // ثبت یک دیسک بازی جدید
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
    {
        try
        {
            var createdProduct = await _productService.CreateProductAsync(dto);

            // کد 201 Created به همراه آدرس متد GetById برای خواندن همین آیتم
            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
        }
        catch (ArgumentException ex)
        {
            // اگر قوانین بیزنس (مثل قیمت منفی) نقض شده بود، خطای 400 Bad Request برمی‌گردانیم
            return BadRequest(new { message = ex.Message });
        }
    }

    // حذف یک دیسک از انبار
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var isDeleted = await _productService.DeleteProductAsync(id);

        if (!isDeleted)
            return NotFound($"محصولی با شناسه {id} جهت حذف یافت نشد.");

        return NoContent(); // کد استاندارد 204
    }
    // ویرایش اطلاعات یک دیسک بازی
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        try
        {
            var isUpdated = await _productService.UpdateProductAsync(id, dto);

            if (!isUpdated)
                return NotFound($"محصولی با شناسه {id} جهت ویرایش یافت نشد.");

            return NoContent(); // کد استاندارد 204: یعنی با موفقیت اعمال شد
        }
        catch (ArgumentException ex)
        {
            // در صورت عدم رعایت قوانین بیزنس، بازگرداندن خطای 400
            return BadRequest(new { message = ex.Message });
        }
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
