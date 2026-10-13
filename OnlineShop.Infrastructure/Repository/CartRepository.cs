// File: OnlineShop.Infrastructure/Repository/CartRepository.cs
using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Interfaces;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Data;
namespace OnlineShop.Infrastructure.Repositories;


public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _context;

    public CartRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByUserIdAsync(string userId)
    {
        // فقط سبد فعال متعلق به کاربر را جست‌وجو می‌کنیم
        return await _context.Carts
            .Where(cart =>
                cart.UserId == userId &&
                cart.IsActive)

            // دریافت آیتم‌های داخل سبد
            .Include(cart => cart.Items)

            // دریافت اطلاعات محصول هر آیتم
            .ThenInclude(item => item.Product)

            // انتخاب سبد فعال؛ در صورت وجود داده تکراری،
            // فعلاً جدیدترین شناسه را انتخاب می‌کنیم
            .OrderByDescending(cart => cart.Id)

            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Cart cart)
    {
        await _context.Carts.AddAsync(cart);
    }

    public async Task AddItemAsync(CartItem cartItem)
    {
        await _context.CartItems.AddAsync(cartItem);
    }

    public Task RemoveItemAsync(CartItem cartItem)
    {
        _context.CartItems.Remove(cartItem);
        return Task.CompletedTask;
    }

    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        return await _context.Products.FindAsync(productId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    // حذف چند آیتم از سبد خرید در یک عملیات
    public void RemoveItems(IEnumerable<CartItem> cartItems)
    {
        // علامت‌گذاری تمام آیتم‌های ورودی برای حذف
        _context.CartItems.RemoveRange(cartItems);
    }
}
