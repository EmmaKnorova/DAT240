using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Cart.Pipelines;

public class GetCartByUserIdTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;

    public GetCartByUserIdTests(DbTest dbTest)
    {
        _dbTest = dbTest;
    }

    [Fact]
    public async Task Handle_UserHasCart_ReturnsCartId()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        
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

        var cartId = Guid.NewGuid();
        var cart = new ShoppingCart(cartId, userId);
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new GetCartByUserId.Handler(context);
        var request = new GetCartByUserId.Request(userId);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response.CartId);
        Assert.Equal(cartId, response.CartId.Value);
    }

    [Fact]
    public async Task Handle_UserHasNoCart_ReturnsNull()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        
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

        var handler = new GetCartByUserId.Handler(context);
        var request = new GetCartByUserId.Request(userId);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Null(response.CartId);
    }

    [Fact]
    public async Task Handle_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        
        var handler = new GetCartByUserId.Handler(context);
        var request = new GetCartByUserId.Request(Guid.NewGuid());

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Null(response.CartId);
    }

    [Fact]
    public async Task Handle_MultipleCartsExist_ReturnsCorrectUserCart()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        
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

        var cartId1 = Guid.NewGuid();
        var cart1 = new ShoppingCart(cartId1, userId1);
        cart1.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m);
        context.ShoppingCarts.Add(cart1);

        var cartId2 = Guid.NewGuid();
        var cart2 = new ShoppingCart(cartId2, userId2);
        cart2.AddItem(itemId: 2, itemName: "Burger", itemPrice: 8.00m);
        context.ShoppingCarts.Add(cart2);

        await context.SaveChangesAsync();

        var handler = new GetCartByUserId.Handler(context);
        var request = new GetCartByUserId.Request(userId1);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response.CartId);
        Assert.Equal(cartId1, response.CartId.Value);
        Assert.NotEqual(cartId2, response.CartId.Value);
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsNull()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        
        var handler = new GetCartByUserId.Handler(context);
        var request = new GetCartByUserId.Request(Guid.NewGuid());

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Null(response.CartId);
    }

    [Fact]
    public void Handle_NullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GetCartByUserId.Handler(null!));
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        
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

        var handler = new GetCartByUserId.Handler(context);
        var request = new GetCartByUserId.Request(userId);
        
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(request, cts.Token)
        );
    }

    [Fact]
    public async Task Handle_UserWithEmptyCart_ReturnsCartId()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        
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

        var cartId = Guid.NewGuid();
        var cart = new ShoppingCart(cartId, userId);
        // Don't add any items - empty cart
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new GetCartByUserId.Handler(context);
        var request = new GetCartByUserId.Request(userId);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response.CartId);
        Assert.Equal(cartId, response.CartId.Value);
    }

    [Fact]
    public async Task Handle_SameUserIdMultipleTimes_ReturnsConsistentResult()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        
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

        var cartId = Guid.NewGuid();
        var cart = new ShoppingCart(cartId, userId);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new GetCartByUserId.Handler(context);
        var request = new GetCartByUserId.Request(userId);

        // Act
        var response1 = await handler.Handle(request, CancellationToken.None);
        var response2 = await handler.Handle(request, CancellationToken.None);
        var response3 = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response1.CartId);
        Assert.NotNull(response2.CartId);
        Assert.NotNull(response3.CartId);
        Assert.Equal(response1.CartId, response2.CartId);
        Assert.Equal(response2.CartId, response3.CartId);
    }
}