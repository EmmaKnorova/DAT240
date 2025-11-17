using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Cart.Pipelines;

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
        
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            UserName = "test@example.com",
            Address = "123 Test St",
            City = "Test City",
            PostalCode = "12345"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var cartId = Guid.NewGuid();

        // Create a cart in the database
        var cart = new ShoppingCart(cartId, userId);
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
        
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            UserName = "test@example.com",
            Address = "123 Test St",
            City = "Test City",
            PostalCode = "12345"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var cartId = Guid.NewGuid();

        // Create cart with items
        var cart = new ShoppingCart(cartId, userId);
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
        
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            UserName = "test@example.com",
            Address = "123 Test St",
            City = "Test City",
            PostalCode = "12345"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var cartId = Guid.NewGuid();

        // Create empty cart
        var cart = new ShoppingCart(cartId, userId);
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
        
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            UserName = "test@example.com",
            Address = "123 Test St",
            City = "Test City",
            PostalCode = "12345"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var cartId = Guid.NewGuid();

        // Create cart with items
        var cart = new ShoppingCart(cartId, userId);
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
        
        var userId1 = Guid.NewGuid();
        var user1 = new User
        {
            Id = userId1,
            Name = "User 1",
            Email = "user1@example.com",
            UserName = "user1@example.com",
            Address = "123 Test St",
            City = "Test City",
            PostalCode = "12345"
        };
        context.Users.Add(user1);

        var userId2 = Guid.NewGuid();
        var user2 = new User
        {
            Id = userId2,
            Name = "User 2",
            Email = "user2@example.com",
            UserName = "user2@example.com",
            Address = "456 Test Ave",
            City = "Test City",
            PostalCode = "12345"
        };
        context.Users.Add(user2);

        var userId3 = Guid.NewGuid();
        var user3 = new User
        {
            Id = userId3,
            Name = "User 3",
            Email = "user3@example.com",
            UserName = "user3@example.com",
            Address = "789 Test Blvd",
            City = "Test City",
            PostalCode = "12345"
        };
        context.Users.Add(user3);
        await context.SaveChangesAsync();

        var targetCartId = Guid.NewGuid();
        var otherCartId1 = Guid.NewGuid();
        var otherCartId2 = Guid.NewGuid();

        // Create multiple carts
        var targetCart = new ShoppingCart(targetCartId, userId1);
        targetCart.AddItem(1, "Pizza", 12.99m);
        
        var otherCart1 = new ShoppingCart(otherCartId1, userId2);
        otherCart1.AddItem(2, "Burger", 8.99m);
        
        var otherCart2 = new ShoppingCart(otherCartId2, userId3);
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