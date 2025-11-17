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

public class AddItemTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;
    private readonly ITestOutputHelper _output;

    public AddItemTests(DbTest dbTest, ITestOutputHelper output)
    {
        _dbTest = dbTest;
        _output = output;
    }

    [Fact]
    public async Task Handle_CreatesNewCart_WhenCartDoesNotExist()
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

        var handler = new AddItem.Handler(context);
        var cartId = Guid.NewGuid();
        var request = new AddItem.Request(
            ItemId: 1,
            ItemName: "Pizza",
            ItemPrice: 12.99m,
            UserId: userId,
            CartId: cartId
        );

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        var cart = await context.Set<ShoppingCart>()
            .Include(c => c.Items)
            .SingleOrDefaultAsync(c => c.Id == cartId);

        Assert.NotNull(cart);
        Assert.Equal(userId, cart.UserId);
        Assert.Single(cart.Items);
        Assert.Equal("Pizza", cart.Items.First().Name);
        Assert.Equal(12.99m, cart.Items.First().Price);
    }

    [Fact]
    public async Task Handle_AddsItemToExistingCart_WhenCartExists()
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

        var cartId = Guid.NewGuid();
        // Create existing cart with userId
        var existingCart = new ShoppingCart(cartId, userId);
        existingCart.AddItem(1, "Burger", 8.99m);
        context.Set<ShoppingCart>().Add(existingCart);
        await context.SaveChangesAsync();

        var handler = new AddItem.Handler(context);
        var request = new AddItem.Request(
            ItemId: 2,
            ItemName: "Fries",
            ItemPrice: 3.99m,
            UserId: userId,
            CartId: cartId
        );

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        var cart = await context.Set<ShoppingCart>()
            .Include(c => c.Items)
            .SingleOrDefaultAsync(c => c.Id == cartId);

        Assert.NotNull(cart);
        Assert.Equal(2, cart.Items.Count());
    }
    
    [Fact]
    public async Task AddNewItemTwice_IncrementsItemCount()
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
        var request = new AddItem.Request(
            ItemId: 1,
            ItemName: "Test",
            ItemPrice: 1m,
            UserId: userId,
            CartId: cartId
        );

        var handler = new AddItem.Handler(context);
        
        // Act - Add same item twice
        await handler.Handle(request, CancellationToken.None);
        await handler.Handle(request, CancellationToken.None);

        // Assert
        var cart = await context.Set<ShoppingCart>()
            .Include(c => c.Items)
            .SingleOrDefaultAsync(c => c.Id == cartId);

        Assert.NotNull(cart);
        Assert.Single(cart.Items); // Only 1 unique item
        Assert.Equal(2, cart.Items.First().Count); // Count is 2
    }

    [Fact]
    public async Task Handle_NullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AddItem.Handler(null!));
    }

    [Fact]
    public async Task Handle_AddsMultipleDifferentItems_CreatesMultipleCartItems()
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
        var handler = new AddItem.Handler(context);

        // Act - Add three different items
        await handler.Handle(new AddItem.Request(1, "Pizza", 12.99m, userId, cartId), CancellationToken.None);
        await handler.Handle(new AddItem.Request(2, "Burger", 8.99m, userId, cartId), CancellationToken.None);
        await handler.Handle(new AddItem.Request(3, "Fries", 3.99m, userId, cartId), CancellationToken.None);

        // Assert
        var cart = await context.Set<ShoppingCart>()
            .Include(c => c.Items)
            .SingleOrDefaultAsync(c => c.Id == cartId);

        Assert.NotNull(cart);
        Assert.Equal(3, cart.Items.Count());
        Assert.Contains(cart.Items, i => i.Name == "Pizza" && i.Count == 1);
        Assert.Contains(cart.Items, i => i.Name == "Burger" && i.Count == 1);
        Assert.Contains(cart.Items, i => i.Name == "Fries" && i.Count == 1);
    }
}