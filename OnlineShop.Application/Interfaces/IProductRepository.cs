// File: Interfaces/IProductRepository.cs
using OnlineShop.Application.DTOs.Products;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Interfaces;

public interface IProductRepository
{
    Task<List<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(int id);
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task<Product?> GetEntityByIdAsync(int id);
    // این متد قراره لیست تمام محصولاتی که متعلق به یک دسته‌بندی مشخص هستند رو از دیتابیس بکشه بیرون
    Task<List<Product>> GetProductsByCategoryIdAsync(int categoryId, CancellationToken ct = default);


}
