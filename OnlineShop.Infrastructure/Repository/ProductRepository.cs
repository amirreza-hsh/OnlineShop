// File: Repositories/ProductRepository.cs
using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.DTOs.Products;
using OnlineShop.Application.Interfaces;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Data;

namespace OnlineShop.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        return await _context.Products
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Title = p.Title,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                Stock = p.Stock,
                Platform = p.Platform,
                Region = p.Region,
                EditionType = p.EditionType,
                CategoryId = p.CategoryId // اضافه شد
            })
            .ToListAsync();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Where(p => p.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Title = p.Title,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                Stock = p.Stock,
                Platform = p.Platform,
                Region = p.Region,
                EditionType = p.EditionType,
                CategoryId = p.CategoryId // اضافه شد
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Product> AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public async Task<Product?> GetEntityByIdAsync(int id)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Product>> GetProductsByCategoryIdAsync(int categoryId, CancellationToken ct = default)
    {
        // با استفاده از Where در LINQ و EF Core، رکوردهایی رو فیلتر می‌کنیم که CategoryId اون‌ها برابره با ورودی متد
        return await _context.Products
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync(ct);
    }
}
