
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Category?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Category> AddAsync(
        Category category,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Category category,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Category category,
        CancellationToken cancellationToken = default);
}
