using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Tests.Helpers;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Cart.Pipelines;

public class ClearCartTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;

    public ClearCartTests(DbTest dbTest)
    {
        _dbTest = dbTest;
    }

    private ShoppingCart CreateTestCart(Guid? userId = null)
    {
        var cartId = Guid.NewGuid();
        var userIdValue = userId ?? Guid.NewGuid();
        return new ShoppingCart(cartId, userIdValue);
    }

    [Fact]
    public async Task Handle_ClearsAllItemsFromCart()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(2, "Burger", 8.00m);
        cart.AddItem(3, "Soda", 3.00m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new ClearCart.Handler(context);
        var command = new ClearCart.Request(cart.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Message);

        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart.Id);

        Assert.NotNull(updatedCart);
        Assert.Empty(updatedCart.Items);
    }

    [Fact]
    public async Task Handle_ClearsCartWithSingleItem()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        cart.AddItem(1, "Pizza", 10.00m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new ClearCart.Handler(context);
        var command = new ClearCart.Request(cart.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);

        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart.Id);

        Assert.Empty(updatedCart!.Items);
    }

    [Fact]
    public async Task Handle_ClearsCartWithMultipleQuantities()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(2, "Burger", 8.00m);
        cart.AddItem(2, "Burger", 8.00m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new ClearCart.Handler(context);
        var command = new ClearCart.Request(cart.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);

        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart.Id);

        Assert.Empty(updatedCart!.Items);
    }

    [Fact]
    public async Task Handle_WithEmptyCart_Succeeds()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new ClearCart.Handler(context);
        var command = new ClearCart.Request(cart.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);

        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart.Id);

        Assert.Empty(updatedCart!.Items);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenCartNotFound()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var nonExistentCartId = Guid.NewGuid();

        var handler = new ClearCart.Handler(context);
        var command = new ClearCart.Request(nonExistentCartId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Cart not found", result.Message);
    }

    [Fact]
    public async Task Handle_SavesChangesToDatabase()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(2, "Burger", 8.00m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new ClearCart.Handler(context);
        var command = new ClearCart.Request(cart.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);

        // Verify changes are persisted
        context.Entry(cart).State = EntityState.Detached;
        
        var reloadedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart.Id);

        Assert.Empty(reloadedCart!.Items);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        cart.AddItem(1, "Pizza", 10.00m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new ClearCart.Handler(context);
        var command = new ClearCart.Request(cart.Id);
        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        var result = await handler.Handle(command, cancellationTokenSource.Token);

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task Handle_CanClearMultipleTimes()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        cart.AddItem(1, "Pizza", 10.00m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new ClearCart.Handler(context);
        var command = new ClearCart.Request(cart.Id);

        // Act - Clear twice
        var result1 = await handler.Handle(command, CancellationToken.None);
        var result2 = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result1.Success);
        Assert.True(result2.Success);

        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart.Id);

        Assert.Empty(updatedCart!.Items);
    }

    [Fact]
    public async Task Handle_PreservesCartProperties()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var userId = Guid.NewGuid();
        var cart = CreateTestCart(userId);
        
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(2, "Burger", 8.00m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var originalCartId = cart.Id;
        var originalUserId = cart.UserId;

        var handler = new ClearCart.Handler(context);
        var command = new ClearCart.Request(cart.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);

        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart.Id);

        Assert.Equal(originalCartId, updatedCart!.Id);
        Assert.Equal(originalUserId, updatedCart.UserId);
    }

    [Fact]
    public async Task Handle_ClearsLargeNumberOfItems()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        // Add 50 different items
        for (int i = 1; i <= 50; i++)
        {
            cart.AddItem(i, $"Item {i}", i * 1.50m);
        }

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new ClearCart.Handler(context);
        var command = new ClearCart.Request(cart.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);

        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart.Id);

        Assert.Empty(updatedCart!.Items);
    }

    [Fact]
    public async Task Handle_DoesNotAffectOtherCarts()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart1 = CreateTestCart();
        var cart2 = CreateTestCart();
        
        cart1.AddItem(1, "Pizza", 10.00m);
        cart1.AddItem(2, "Burger", 8.00m);
        
        cart2.AddItem(3, "Soda", 3.00m);
        cart2.AddItem(4, "Fries", 4.00m);

        await context.ShoppingCarts.AddRangeAsync(cart1, cart2);
        await context.SaveChangesAsync();

        var handler = new ClearCart.Handler(context);
        var command = new ClearCart.Request(cart1.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);

        var updatedCart1 = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart1.Id);
        
        var updatedCart2 = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart2.Id);

        Assert.Empty(updatedCart1!.Items);
        Assert.Equal(2, updatedCart2!.Items.Count());
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ClearCart.Handler(null!));
        Assert.Equal("db", exception.ParamName);
    }

    [Fact]
    public async Task Handle_AfterClear_CanAddNewItems()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(2, "Burger", 8.00m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new ClearCart.Handler(context);
        var command = new ClearCart.Request(cart.Id);

        // Act - Clear cart
        var result = await handler.Handle(command, CancellationToken.None);

        // Act - Add new items
        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart.Id);
        
        updatedCart!.AddItem(3, "Soda", 3.00m);
        await context.SaveChangesAsync();

        // Assert
        Assert.True(result.Success);
        
        var finalCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart.Id);

        Assert.Single(finalCart!.Items);
        Assert.Equal(3, finalCart.Items.First().Sku);
        Assert.Equal("Soda", finalCart.Items.First().Name);
    }

    [Fact]
    public async Task Handle_ClearsCartWithMixedPrices()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        cart.AddItem(1, "Expensive Item", 99.99m);
        cart.AddItem(2, "Cheap Item", 0.50m);
        cart.AddItem(3, "Medium Item", 15.75m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new ClearCart.Handler(context);
        var command = new ClearCart.Request(cart.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);

        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart.Id);

        Assert.Empty(updatedCart!.Items);
    }

    [Fact]
    public async Task Handle_SucceedsWithAlreadyEmptyCart()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        // Add and then clear manually
        cart.AddItem(1, "Pizza", 10.00m);
        cart.ClearCart();

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new ClearCart.Handler(context);
        var command = new ClearCart.Request(cart.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);

        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart.Id);

        Assert.Empty(updatedCart!.Items);
    }
}