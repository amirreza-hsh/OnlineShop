using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Application.Interfaces;

namespace OnlineShop.Tests;

public class MissingCartItemService : ICartService
{
    private readonly Exception _exception;

    public MissingCartItemService(Exception exception)
    {
        _exception = exception;
    }

    public Task RemoveFromCartAsync(string userId, int productId)
    {
        return Task.FromException(_exception);
    }

    public Task<CartDto> GetCartByUserIdAsync(string userId)
        => throw new NotSupportedException();

    public Task AddToCartAsync(
        string userId,
        AddToCartDto dto)
        => throw new NotSupportedException();

    public Task UpdateItemQuantityAsync(
        string userId,
        int productId,
        UpdateCartItemQuantityDto dto)
        => throw new NotSupportedException();

    public Task ClearCartAsync(string userId)
        => throw new NotSupportedException();
}