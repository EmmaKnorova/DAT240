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

public class RemoveItemCompletelyTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;

    public RemoveItemCompletelyTests(DbTest dbTest)
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
    public async Task Handle_RemovesEntireItemLine_RegardlessOfQuantity()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        // Add item with quantity 5
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(1, "Pizza", 10.00m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new RemoveItemCompletely.Handler(context);
        var command = new RemoveItemCompletely.Request(cart.Id, 1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Message);

        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart.Id);

        Assert.Empty(updatedCart!.Items);
    }

    [Fact]
    public async Task Handle_RemovesItemWithQuantityOne()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        cart.AddItem(1, "Pizza", 10.00m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new RemoveItemCompletely.Handler(context);
        var command = new RemoveItemCompletely.Request(cart.Id, 1);

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

        var handler = new RemoveItemCompletely.Handler(context);
        var command = new RemoveItemCompletely.Request(nonExistentCartId, 1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Cart not found", result.Message);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenItemNotInCart()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        cart.AddItem(1, "Pizza", 10.00m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new RemoveItemCompletely.Handler(context);
        var command = new RemoveItemCompletely.Request(cart.Id, 999);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Item with SKU 999 not found in cart", result.Message);
    }

    [Fact]
    public async Task Handle_OnlyRemovesSpecifiedItem_LeavesOtherItems()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(2, "Burger", 8.00m);
        cart.AddItem(2, "Burger", 8.00m);
        cart.AddItem(3, "Soda", 3.00m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new RemoveItemCompletely.Handler(context);
        var command = new RemoveItemCompletely.Request(cart.Id, 1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);

        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart.Id);

        Assert.Equal(2, updatedCart!.Items.Count());
        Assert.DoesNotContain(updatedCart.Items, i => i.Sku == 1);
        
        var burgerItem = updatedCart.Items.First(i => i.Sku == 2);
        var sodaItem = updatedCart.Items.First(i => i.Sku == 3);

        Assert.Equal(2, burgerItem.Count);
        Assert.Equal(1, sodaItem.Count);
    }

    [Fact]
    public async Task Handle_SavesChangesToDatabase()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(1, "Pizza", 10.00m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new RemoveItemCompletely.Handler(context);
        var command = new RemoveItemCompletely.Request(cart.Id, 1);

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
        cart.AddItem(1, "Pizza", 10.00m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new RemoveItemCompletely.Handler(context);
        var command = new RemoveItemCompletely.Request(cart.Id, 1);
        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        var result = await handler.Handle(command, cancellationTokenSource.Token);

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task Handle_RemovesMultipleItemsSequentially()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(2, "Burger", 8.00m);
        cart.AddItem(3, "Soda", 3.00m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new RemoveItemCompletely.Handler(context);

        // Act - Remove all items one by one
        var result1 = await handler.Handle(new RemoveItemCompletely.Request(cart.Id, 1), CancellationToken.None);
        var result2 = await handler.Handle(new RemoveItemCompletely.Request(cart.Id, 2), CancellationToken.None);
        var result3 = await handler.Handle(new RemoveItemCompletely.Request(cart.Id, 3), CancellationToken.None);

        // Assert
        Assert.True(result1.Success);
        Assert.True(result2.Success);
        Assert.True(result3.Success);

        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart.Id);

        Assert.Empty(updatedCart!.Items);
    }

    [Fact]
    public async Task Handle_WithEmptyCart_ReturnsFailure()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new RemoveItemCompletely.Handler(context);
        var command = new RemoveItemCompletely.Request(cart.Id, 1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Item with SKU 1 not found in cart", result.Message);
    }

    [Fact]
    public async Task Handle_PreservesOtherCartProperties()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var userId = Guid.NewGuid();
        var cart = CreateTestCart(userId);
        
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(1, "Pizza", 10.00m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var originalCartId = cart.Id;
        var originalUserId = cart.UserId;

        var handler = new RemoveItemCompletely.Handler(context);
        var command = new RemoveItemCompletely.Request(cart.Id, 1);

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
    public async Task Handle_WithDifferentItemSkus_RemovesCorrectItem()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        cart.AddItem(100, "Pizza", 10.00m);
        cart.AddItem(200, "Burger", 8.00m);
        cart.AddItem(300, "Soda", 3.00m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new RemoveItemCompletely.Handler(context);
        var command = new RemoveItemCompletely.Request(cart.Id, 200);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);

        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart.Id);

        Assert.Equal(2, updatedCart!.Items.Count());
        Assert.DoesNotContain(updatedCart.Items, i => i.Sku == 200);
        Assert.Contains(updatedCart.Items, i => i.Sku == 100);
        Assert.Contains(updatedCart.Items, i => i.Sku == 300);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new RemoveItemCompletely.Handler(null!));
        Assert.Equal("db", exception.ParamName);
    }

    [Fact]
    public async Task Handle_RemovesItemWithHighQuantity()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        // Add 100 pizzas
        for (int i = 0; i < 100; i++)
        {
            cart.AddItem(1, "Pizza", 10.00m);
        }

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new RemoveItemCompletely.Handler(context);
        var command = new RemoveItemCompletely.Request(cart.Id, 1);

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
    public async Task Handle_DoesNotAffectItemPricesOfRemainingItems()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(2, "Burger", 8.50m);
        cart.AddItem(3, "Soda", 3.25m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new RemoveItemCompletely.Handler(context);
        var command = new RemoveItemCompletely.Request(cart.Id, 1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);

        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart.Id);

        var burgerItem = updatedCart!.Items.First(i => i.Sku == 2);
        var sodaItem = updatedCart.Items.First(i => i.Sku == 3);

        Assert.Equal(8.50m, burgerItem.Price);
        Assert.Equal(3.25m, sodaItem.Price);
        Assert.Equal(8.50m, burgerItem.Sum);
        Assert.Equal(3.25m, sodaItem.Sum);
    }

    [Fact]
    public async Task Handle_RemovesItemWithDecimalPrice()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        cart.AddItem(1, "Pizza", 12.99m);
        cart.AddItem(1, "Pizza", 12.99m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new RemoveItemCompletely.Handler(context);
        var command = new RemoveItemCompletely.Request(cart.Id, 1);

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
    public async Task Handle_CanRemoveLastItemInCart()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = CreateTestCart();
        
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(2, "Burger", 8.00m);

        await context.ShoppingCarts.AddAsync(cart);
        await context.SaveChangesAsync();

        var handler = new RemoveItemCompletely.Handler(context);

        // Act - Remove first item
        await handler.Handle(new RemoveItemCompletely.Request(cart.Id, 1), CancellationToken.None);
        
        // Act - Remove last item
        var result = await handler.Handle(new RemoveItemCompletely.Request(cart.Id, 2), CancellationToken.None);

        // Assert
        Assert.True(result.Success);

        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart.Id);

        Assert.Empty(updatedCart!.Items);
    }
}