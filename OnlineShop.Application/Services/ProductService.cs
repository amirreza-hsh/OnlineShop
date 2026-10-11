// File: Services/ProductService.cs
using OnlineShop.Application.DTOs.Products;
using OnlineShop.Application.Interfaces;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    // اصل Dependency Injection: سرویس به قرارداد تکیه می‌کند نه به پیاده‌سازی مستقیم
    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        // اعمال اعتبارسنجی منطق بیزنس (Business Validation)
        if (dto.Price < 0)
            throw new ArgumentException("قیمت دیسک نمی‌تواند منفی باشد.");

        if (dto.DiscountPrice.HasValue && dto.DiscountPrice.Value >= dto.Price)
            throw new ArgumentException("قیمت با تخفیف باید از قیمت اصلی کمتر باشد.");

        if (dto.Stock < 0)
            throw new ArgumentException("موجودی انبار نمی‌تواند منفی باشد.");

        // تبدیل (Mapping) از DTO به Domain Entity
        var product = new Product
        {
            Title = dto.Title,
            Price = dto.Price,
            DiscountPrice = dto.DiscountPrice,
            Stock = dto.Stock,
            Platform = dto.Platform,
            Region = dto.Region,
            EditionType = dto.EditionType,
            CategoryId = dto.CategoryId // حتماً آیدی دسته رو هم ست کن
        };

        var createdProduct = await _repository.AddAsync(product);

        // تبدیل نتیجه به DTO برای بازگرداندن به لایه بالاتر
        return new ProductDto
        {
            Id = createdProduct.Id,
            Title = createdProduct.Title,
            Price = createdProduct.Price,
            DiscountPrice = createdProduct.DiscountPrice,
            Stock = createdProduct.Stock,
            Platform = createdProduct.Platform,
            Region = createdProduct.Region,
            EditionType = createdProduct.EditionType,
            CategoryId = createdProduct.CategoryId // 👈 اضافه شد: تا شناسه دسته‌بندی در خروجی پاسخ صفر نباشد
        };
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        // ابتدا بررسی می‌کنیم آیتم وجود دارد یا خیر
        var productDto = await _repository.GetByIdAsync(id);
        if (productDto == null)
            return false;

        // ساخت انتیتی حداقلی برای حذف
        var product = new Product { Id = id };
        await _repository.DeleteAsync(product);
        return true;
    }

    public async Task<bool> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        // ۱. اعتبارسنجی قوانین بیزنس
        if (dto.Price < 0)
            throw new ArgumentException("قیمت دیسک نمی‌تواند منفی باشد.");

        if (dto.DiscountPrice.HasValue && dto.DiscountPrice.Value >= dto.Price)
            throw new ArgumentException("قیمت با تخفیف باید از قیمت اصلی کمتر باشد.");

        if (dto.Stock < 0)
            throw new ArgumentException("موجودی انبار نمی‌تواند منفی باشد.");

        // ۲. گرفتن محصول موجود از دیتابیس
        var product = await _repository.GetEntityByIdAsync(id);
        if (product == null)
            return false;

        // ۳. اعمال تغییرات روی همان entity (بدون ساخت آبجکت جدید)
        product.Title = dto.Title;
        product.Price = dto.Price;
        product.DiscountPrice = dto.DiscountPrice;
        product.Stock = dto.Stock;
        product.Platform = dto.Platform;
        product.Region = dto.Region;
        product.EditionType = dto.EditionType;
        product.CategoryId = dto.CategoryId;

        await _repository.UpdateAsync(product);
        return true;
    }

    // این متد محصولات یک دسته‌بندی خاص را از ریپازیتوری می‌گیرد و به DTO تبدیل می‌کند
    // پیاده‌سازی متد فیلتر محصولات بر اساس دسته‌بندی
    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(int categoryId, CancellationToken ct = default)
    {
        // فراخوانی متد صحیح از لایه ریپازیتوری با نام دقیق خودش
        var products = await _repository.GetProductsByCategoryIdAsync(categoryId, ct);

        // تبدیل (مپ کردن) انتیتی‌ها به DTO برای ارسال به لایه‌های بالاتر
        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Title = p.Title,
            Price = p.Price,
            DiscountPrice = p.DiscountPrice,
            Stock = p.Stock,
            Platform = p.Platform,
            Region = p.Region,
            EditionType = p.EditionType,
            CategoryId = p.CategoryId
        });
    }
}
