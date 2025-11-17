using System;
using System.Threading;
using System.Threading.Tasks;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Cart.Pipelines;

public class GetItemCountTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;
    private readonly ITestOutputHelper _output;

    public GetItemCountTests(DbTest dbTest, ITestOutputHelper output)
    {
        _dbTest = dbTest;
        _output = output;
    }

    [Fact]
    public async Task NoCart_ShouldReturn0()
    {
        // Arrange
        await using var context = _dbTest.CreateContext();
        var cartId = Guid.NewGuid();
        var request = new GetItemCount.Request(cartId);
        var handler = new GetItemCount.Handler(context);

        // Act
        var itemCount = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal(0, itemCount);
    }

    [Fact]
    public async Task CartWithOneItem_ShouldReturn1()
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

        // Add one item to cart
        var addHandler = new AddItem.Handler(context);
        var addRequest = new AddItem.Request(1, "Test", 1m, userId, cartId);
        await addHandler.Handle(addRequest, CancellationToken.None);

        var getCountHandler = new GetItemCount.Handler(context);
        var getCountRequest = new GetItemCount.Request(cartId);

        // Act
        var itemCount = await getCountHandler.Handle(getCountRequest, CancellationToken.None);

        // Assert
        Assert.Equal(1, itemCount);
    }

    [Fact]
    public async Task TwoCartsWithOneItemEach_ShouldReturn1()
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
        await context.SaveChangesAsync();

        var cart1Id = Guid.NewGuid();
        var cart2Id = Guid.NewGuid();

        var addHandler = new AddItem.Handler(context);

        // Add item to first cart
        var request1 = new AddItem.Request(1, "Test", 1m, userId1, cart1Id);
        await addHandler.Handle(request1, CancellationToken.None);

        // Add item to second cart
        var request2 = new AddItem.Request(1, "Test", 1m, userId2, cart2Id);
        await addHandler.Handle(request2, CancellationToken.None);

        var getCountHandler = new GetItemCount.Handler(context);
        var getCountRequest = new GetItemCount.Request(cart1Id);

        // Act - Get count for first cart only
        var itemCount = await getCountHandler.Handle(getCountRequest, CancellationToken.None);

        // Assert - Should only count items in first cart
        Assert.Equal(1, itemCount);
    }

    [Fact]
    public async Task CartWithTwoItems_ShouldReturn2()
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

        var addHandler = new AddItem.Handler(context);

        // Add two different items
        var request1 = new AddItem.Request(1, "Test", 1m, userId, cartId);
        await addHandler.Handle(request1, CancellationToken.None);

        var request2 = new AddItem.Request(2, "Test 2", 1m, userId, cartId);
        await addHandler.Handle(request2, CancellationToken.None);

        var getCountHandler = new GetItemCount.Handler(context);
        var getCountRequest = new GetItemCount.Request(cartId);

        // Act
        var itemCount = await getCountHandler.Handle(getCountRequest, CancellationToken.None);

        // Assert
        Assert.Equal(2, itemCount);
    }

    [Fact]
    public async Task CartWithTwoItemsWithTwoOfOne_ShouldReturn3()
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

        var addHandler = new AddItem.Handler(context);

        // Add first item
        var request1 = new AddItem.Request(1, "Test", 1m, userId, cartId);
        await addHandler.Handle(request1, CancellationToken.None);

        // Add second item
        var request2 = new AddItem.Request(2, "Test 2", 1m, userId, cartId);
        await addHandler.Handle(request2, CancellationToken.None);

        // Add second item again (should increment count)
        var request3 = new AddItem.Request(2, "Test 2", 1m, userId, cartId);
        await addHandler.Handle(request3, CancellationToken.None);

        var getCountHandler = new GetItemCount.Handler(context);
        var getCountRequest = new GetItemCount.Request(cartId);

        // Act
        var itemCount = await getCountHandler.Handle(getCountRequest, CancellationToken.None);

        // Assert - Should be 3 (1 + 2)
        Assert.Equal(3, itemCount);
    }

    [Fact]
    public async Task EmptyCart_ShouldReturn0()
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

        var handler = new GetItemCount.Handler(context);
        var request = new GetItemCount.Request(cartId);

        // Act
        var itemCount = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal(0, itemCount);
    }
}