// File: Interfaces/IProductService.cs
using OnlineShop.Application.DTOs.Products;


namespace OnlineShop.Application.Interfaces;

public interface IProductService
{
    // دریافت همه محصولات
    Task<List<ProductDto>> GetAllProductsAsync();

    // دریافت تک محصول با شناسه
    Task<ProductDto?> GetProductByIdAsync(int id);

    // افزودن محصول جدید
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);

    // ویرایش محصول
    Task<bool> UpdateProductAsync(int id, UpdateProductDto dto);

    // حذف محصول
    Task<bool> DeleteProductAsync(int id);

    // دریافت محصولات بر اساس شناسه دسته‌بندی
    Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(int categoryId, CancellationToken ct = default);
}

