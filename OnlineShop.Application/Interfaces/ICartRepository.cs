
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(string userId);

    Task AddAsync(Cart cart);

    Task AddItemAsync(CartItem cartItem);

    Task RemoveItemAsync(CartItem cartItem);

    Task<Product?> GetProductByIdAsync(int productId);

    Task SaveChangesAsync();
    // حذف مجموعه‌ای از آیتم‌های سبد خرید
    void RemoveItems(IEnumerable<CartItem> cartItems);
}
