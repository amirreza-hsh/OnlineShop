using OnlineShop.Application.DTOs.Categories;
using OnlineShop.Application.DTOs.Products;

namespace OnlineShop.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllCategoriesAsync(CancellationToken ct = default);
    Task<CategoryDto?> GetCategoryByIdAsync(int id, CancellationToken ct = default);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken ct = default);
    Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto dto, CancellationToken ct = default);
    Task<bool> DeleteCategoryAsync(int id, CancellationToken ct = default);
    // دریافت لیست محصولات بر اساس شناسه دسته‌بندی
}
