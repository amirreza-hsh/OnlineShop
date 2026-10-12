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
        return await _context.Carts
            // بارگذاری آیتم‌ها و محصول هر آیتم
            .Include(cart => cart.Items)
            .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(cart => cart.UserId == userId);
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
}
