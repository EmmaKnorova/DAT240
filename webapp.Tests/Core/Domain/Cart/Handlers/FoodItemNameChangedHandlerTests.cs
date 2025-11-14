using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Handlers;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products.Events;
using TarlBreuJacoBaraKnor.webapp.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Cart.Handlers;

public class FoodItemNameChangedHandlerTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;

    public FoodItemNameChangedHandlerTests(DbTest dbTest)
    {
        _dbTest = dbTest;
    }

    [Fact]
    public async Task Handle_SingleCartWithMatchingItem_UpdatesItemName()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(itemId: 1, itemName: "Old Pizza", itemPrice: 10.00m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new FoodItemNameChangedHandler(context);
        var notification = new FoodItemNameChanged(itemId: 1, oldName: "Old Pizza", newName: "New Pizza");

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstAsync(c => c.Id == cart.Id);
        
        var updatedItem = updatedCart.Items.First();
        Assert.Equal("New Pizza", updatedItem.Name);
    }

    [Fact]
    public async Task Handle_MultipleCartsWithMatchingItem_UpdatesAllItems()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart1 = new ShoppingCart(Guid.NewGuid());
        cart1.AddItem(itemId: 1, itemName: "Old Pizza", itemPrice: 10.00m);
        
        var cart2 = new ShoppingCart(Guid.NewGuid());
        cart2.AddItem(itemId: 1, itemName: "Old Pizza", itemPrice: 10.00m);
        
        context.ShoppingCarts.AddRange(cart1, cart2);
        await context.SaveChangesAsync();

        var handler = new FoodItemNameChangedHandler(context);
        var notification = new FoodItemNameChanged(itemId: 1, oldName: "Old Pizza", newName: "Updated Pizza");

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var allCarts = await context.ShoppingCarts
            .Include(c => c.Items)
            .ToListAsync();

        foreach (var cart in allCarts)
        {
            var item = cart.Items.First(i => i.Sku == 1);
            Assert.Equal("Updated Pizza", item.Name);
        }
    }

    [Fact]
    public async Task Handle_CartWithMultipleItems_UpdatesOnlyMatchingItem()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(itemId: 1, itemName: "Old Pizza", itemPrice: 10.00m);
        cart.AddItem(itemId: 2, itemName: "Burger", itemPrice: 8.00m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new FoodItemNameChangedHandler(context);
        var notification = new FoodItemNameChanged(itemId: 1, oldName: "Old Pizza", newName: "New Pizza");

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstAsync(c => c.Id == cart.Id);
        
        var pizzaItem = updatedCart.Items.First(i => i.Sku == 1);
        var burgerItem = updatedCart.Items.First(i => i.Sku == 2);
        
        Assert.Equal("New Pizza", pizzaItem.Name);
        Assert.Equal("Burger", burgerItem.Name); // Should remain unchanged
    }

    [Fact]
    public async Task Handle_NoMatchingItems_DoesNothing()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(itemId: 2, itemName: "Burger", itemPrice: 8.00m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new FoodItemNameChangedHandler(context);
        var notification = new FoodItemNameChanged(itemId: 1, oldName: "Old Pizza", newName: "New Pizza");

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var unchangedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstAsync(c => c.Id == cart.Id);
        
        var burgerItem = unchangedCart.Items.First();
        Assert.Equal("Burger", burgerItem.Name); // Should remain unchanged
        Assert.Equal(2, burgerItem.Sku);
    }

    [Fact]
    public async Task Handle_EmptyDatabase_DoesNotThrow()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var handler = new FoodItemNameChangedHandler(context);
        var notification = new FoodItemNameChanged(itemId: 1, oldName: "Old Pizza", newName: "New Pizza");

        // Act & Assert - Should not throw
        await handler.Handle(notification, CancellationToken.None);
        
        // Verify no carts exist
        Assert.Empty(context.ShoppingCarts);
    }

    [Fact]
    public async Task Handle_CartWithDuplicateItemSku_UpdatesBothInstances()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(itemId: 1, itemName: "Old Pizza", itemPrice: 10.00m);
        cart.AddItem(itemId: 1, itemName: "Old Pizza", itemPrice: 10.00m); // Adds to count, doesn't create duplicate
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new FoodItemNameChangedHandler(context);
        var notification = new FoodItemNameChanged(itemId: 1, oldName: "Old Pizza", newName: "New Pizza");

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstAsync(c => c.Id == cart.Id);
        
        // Should only have one item with updated name and count of 2
        Assert.Single(updatedCart.Items);
        var item = updatedCart.Items.First();
        Assert.Equal("New Pizza", item.Name);
        Assert.Equal(2, item.Count);
    }

    [Fact]
    public async Task Handle_NullContext_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FoodItemNameChangedHandler(null!));
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(itemId: 1, itemName: "Old Pizza", itemPrice: 10.00m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new FoodItemNameChangedHandler(context);
        var notification = new FoodItemNameChanged(itemId: 1, oldName: "Old Pizza", newName: "New Pizza");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(notification, cts.Token)
        );
    }

    [Fact]
    public async Task Handle_MultipleItemsInSameCart_UpdatesAllMatchingItems()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(itemId: 1, itemName: "Old Pizza", itemPrice: 10.00m);
        cart.AddItem(itemId: 2, itemName: "Burger", itemPrice: 8.00m);
        cart.AddItem(itemId: 3, itemName: "Salad", itemPrice: 6.00m);
        
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new FoodItemNameChangedHandler(context);
        var notification = new FoodItemNameChanged(itemId: 1, oldName: "Old Pizza", newName: "Deluxe Pizza");

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstAsync(c => c.Id == cart.Id);
        
        Assert.Equal(3, updatedCart.Items.Count());
        Assert.Equal("Deluxe Pizza", updatedCart.Items.First(i => i.Sku == 1).Name);
        Assert.Equal("Burger", updatedCart.Items.First(i => i.Sku == 2).Name);
        Assert.Equal("Salad", updatedCart.Items.First(i => i.Sku == 3).Name);
    }

    [Fact]
    public async Task Handle_SavesChangesToDatabase()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(itemId: 1, itemName: "Old Pizza", itemPrice: 10.00m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new FoodItemNameChangedHandler(context);
        var notification = new FoodItemNameChanged(itemId: 1, oldName: "Old Pizza", newName: "New Pizza");

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert - Detach and reload from same context to verify persistence
        context.Entry(cart).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        
        var persistedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstAsync(c => c.Id == cart.Id);
        
        var persistedItem = persistedCart.Items.First();
        Assert.Equal("New Pizza", persistedItem.Name);
    }
}