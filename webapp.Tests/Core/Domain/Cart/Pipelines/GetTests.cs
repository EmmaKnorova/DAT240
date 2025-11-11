using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UiS.Dat240.Lab3.Core.Domain.Cart;
using UiS.Dat240.Lab3.Core.Domain.Cart.Pipelines;
using TarlBreuJacoBaraKnor.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace TarlBreuJacoBaraKnor.Tests.Core.Domain.Cart.Pipelines;

public class GetTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;
    private readonly ITestOutputHelper _output;

    public GetTests(DbTest dbTest, ITestOutputHelper output)
    {
        _dbTest = dbTest;
        _output = output;
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenCartDoesNotExist()
    {
        // Arrange
        await using var context = _dbTest.CreateContext();
        var handler = new Get.Handler(context);
        var nonExistentCartId = Guid.NewGuid();
        var request = new Get.Request(nonExistentCartId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ReturnsCart_WhenCartExists()
    {
        // Arrange
        await using var context = _dbTest.CreateContext();
        var cartId = Guid.NewGuid();

        // Create a cart in the database
        var cart = new ShoppingCart(cartId);
        context.Set<ShoppingCart>().Add(cart);
        await context.SaveChangesAsync();

        var handler = new Get.Handler(context);
        var request = new Get.Request(cartId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cartId, result.Id);
    }

    [Fact]
    public async Task Handle_ReturnsCartWithItems_WhenCartHasItems()
    {
        // Arrange
        await using var context = _dbTest.CreateContext();
        var cartId = Guid.NewGuid();

        // Create cart with items
        var cart = new ShoppingCart(cartId);
        cart.AddItem(1, "Pizza", 12.99m);
        cart.AddItem(2, "Burger", 8.99m);
        cart.AddItem(3, "Fries", 3.99m);
        
        context.Set<ShoppingCart>().Add(cart);
        await context.SaveChangesAsync();

        var handler = new Get.Handler(context);
        var request = new Get.Request(cartId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cartId, result.Id);
        Assert.Equal(3, result.Items.Count());
        
        Assert.Contains(result.Items, i => i.Name == "Pizza" && i.Price == 12.99m);
        Assert.Contains(result.Items, i => i.Name == "Burger" && i.Price == 8.99m);
        Assert.Contains(result.Items, i => i.Name == "Fries" && i.Price == 3.99m);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyCart_WhenCartHasNoItems()
    {
        // Arrange
        await using var context = _dbTest.CreateContext();
        var cartId = Guid.NewGuid();

        // Create empty cart
        var cart = new ShoppingCart(cartId);
        context.Set<ShoppingCart>().Add(cart);
        await context.SaveChangesAsync();

        var handler = new Get.Handler(context);
        var request = new Get.Request(cartId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cartId, result.Id);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Handle_IncludesItemsInQuery_VerifyEagerLoading()
    {
        // Arrange
        await using var context = _dbTest.CreateContext();
        var cartId = Guid.NewGuid();

        // Create cart with items
        var cart = new ShoppingCart(cartId);
        cart.AddItem(1, "Pizza", 12.99m);
        cart.AddItem(1, "Pizza", 12.99m); // Add same item twice to increment count
        
        context.Set<ShoppingCart>().Add(cart);
        await context.SaveChangesAsync();

        var handler = new Get.Handler(context);
        var request = new Get.Request(cartId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert - Items should be loaded (not lazy-loaded)
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(2, result.Items.First().Count); // Count should be 2
        Assert.Equal("Pizza", result.Items.First().Name);
    }

    [Fact]
    public async Task Handle_DoesNotReturnOtherCarts_WhenMultipleCartsExist()
    {
        // Arrange
        await using var context = _dbTest.CreateContext();
        var targetCartId = Guid.NewGuid();
        var otherCartId1 = Guid.NewGuid();
        var otherCartId2 = Guid.NewGuid();

        // Create multiple carts
        var targetCart = new ShoppingCart(targetCartId);
        targetCart.AddItem(1, "Pizza", 12.99m);
        
        var otherCart1 = new ShoppingCart(otherCartId1);
        otherCart1.AddItem(2, "Burger", 8.99m);
        
        var otherCart2 = new ShoppingCart(otherCartId2);
        otherCart2.AddItem(3, "Fries", 3.99m);

        context.Set<ShoppingCart>().Add(targetCart);
        context.Set<ShoppingCart>().Add(otherCart1);
        context.Set<ShoppingCart>().Add(otherCart2);
        await context.SaveChangesAsync();

        var handler = new Get.Handler(context);
        var request = new Get.Request(targetCartId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(targetCartId, result.Id);
        Assert.Single(result.Items);
        Assert.Equal("Pizza", result.Items.First().Name);
    }
}