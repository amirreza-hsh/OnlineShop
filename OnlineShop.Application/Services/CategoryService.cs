using OnlineShop.Application.DTOs.Categories;
using OnlineShop.Application.Interfaces;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync(CancellationToken ct = default)
    {
        var categories = await _repository.GetAllAsync(ct);
        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        }).ToList();
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id, CancellationToken ct = default)
    {
        var category = await _repository.GetByIdAsync(id, ct);
        if (category is null) return null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken ct = default)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        var created = await _repository.AddAsync(category, ct);
        return new CategoryDto { Id = created.Id, Name = created.Name, Description = created.Description };
    }

    public async Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto dto, CancellationToken ct = default)
    {
        var category = await _repository.GetByIdAsync(id, ct);
        if (category is null) return false;

        category.Name = dto.Name;
        category.Description = dto.Description;

        await _repository.UpdateAsync(category, ct);
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(int id, CancellationToken ct = default)
    {
        var category = await _repository.GetByIdAsync(id, ct);
        if (category is null) return false;

        await _repository.DeleteAsync(category, ct);
        return true;
    }
}
