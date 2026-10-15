using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OnlineShop.Application.Interfaces;
using Xunit;

namespace OnlineShop.Tests;

public class CartEndpointsTests
{
    [Fact]
    public async Task RemoveMissingItem_ShouldReturn404ProblemDetails()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");

                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<ICartService>();

                    services.AddScoped<ICartService>(_ =>
                        new MissingCartItemService(
                            new KeyNotFoundException(
                                "محصول در سبد خرید یافت نشد.")));
                });
            });

        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false
            });

        // Act
        using var response = await client.DeleteAsync(
            "/api/cart/test/items/4");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content
            .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
        Assert.Equal(
            "محصول در سبد خرید یافت نشد.",
            problem.Title);
    }

    [Fact]
    public async Task UnexpectedError_ShouldReturn500WithoutInternalDetails()
    {
        // Arrange
        const string internalMessage =
            "Database connection failed: internal-server-name";

        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");

                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<ICartService>();

                    services.AddScoped<ICartService>(_ =>
                        new MissingCartItemService(
                            new InvalidOperationException(internalMessage)));
                });
            });

        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false
            });

        // Act
        using var response = await client.DeleteAsync(
            "/api/cart/test/items/4");

        // Assert
        Assert.Equal(
            HttpStatusCode.InternalServerError,
            response.StatusCode);

        var problem = await response.Content
            .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.Equal(
            "خطای غیرمنتظره‌ای در سرور رخ داد.",
            problem.Title);

        string responseBody =
            await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain(internalMessage, responseBody);
    }
}