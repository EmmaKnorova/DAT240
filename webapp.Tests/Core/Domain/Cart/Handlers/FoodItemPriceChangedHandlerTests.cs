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

public class FoodItemPriceChangedHandlerTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;

    public FoodItemPriceChangedHandlerTests(DbTest dbTest)
    {
        _dbTest = dbTest;
    }

    [Fact]
    public async Task Handle_SingleCartWithMatchingItem_UpdatesItemPrice()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new FoodItemPriceChangedHandler(context);
        var notification = new FoodItemPriceChanged(itemId: 1, oldPrice: 10.00m, newPrice: 12.99m);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstAsync(c => c.Id == cart.Id);
        
        var updatedItem = updatedCart.Items.First();
        Assert.Equal(12.99m, updatedItem.Price);
    }

    [Fact]
    public async Task Handle_MultipleCartsWithMatchingItem_UpdatesAllItems()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart1 = new ShoppingCart(Guid.NewGuid());
        cart1.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m);
        
        var cart2 = new ShoppingCart(Guid.NewGuid());
        cart2.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m);
        
        var cart3 = new ShoppingCart(Guid.NewGuid());
        cart3.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m);
        
        context.ShoppingCarts.AddRange(cart1, cart2, cart3);
        await context.SaveChangesAsync();

        var handler = new FoodItemPriceChangedHandler(context);
        var notification = new FoodItemPriceChanged(itemId: 1, oldPrice: 10.00m, newPrice: 15.50m);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var allCarts = await context.ShoppingCarts
            .Include(c => c.Items)
            .ToListAsync();

        foreach (var cart in allCarts)
        {
            var item = cart.Items.First(i => i.Sku == 1);
            Assert.Equal(15.50m, item.Price);
        }
    }

    [Fact]
    public async Task Handle_CartWithMultipleItems_UpdatesOnlyMatchingItem()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m);
        cart.AddItem(itemId: 2, itemName: "Burger", itemPrice: 8.00m);
        cart.AddItem(itemId: 3, itemName: "Salad", itemPrice: 6.00m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new FoodItemPriceChangedHandler(context);
        var notification = new FoodItemPriceChanged(itemId: 1, oldPrice: 10.00m, newPrice: 14.99m);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstAsync(c => c.Id == cart.Id);
        
        var pizzaItem = updatedCart.Items.First(i => i.Sku == 1);
        var burgerItem = updatedCart.Items.First(i => i.Sku == 2);
        var saladItem = updatedCart.Items.First(i => i.Sku == 3);
        
        Assert.Equal(14.99m, pizzaItem.Price);
        Assert.Equal(8.00m, burgerItem.Price); // Should remain unchanged
        Assert.Equal(6.00m, saladItem.Price); // Should remain unchanged
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

        var handler = new FoodItemPriceChangedHandler(context);
        var notification = new FoodItemPriceChanged(itemId: 1, oldPrice: 10.00m, newPrice: 12.00m);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var unchangedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstAsync(c => c.Id == cart.Id);
        
        var burgerItem = unchangedCart.Items.First();
        Assert.Equal(8.00m, burgerItem.Price); // Should remain unchanged
        Assert.Equal(2, burgerItem.Sku);
    }

    [Fact]
    public async Task Handle_EmptyDatabase_DoesNotThrow()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var handler = new FoodItemPriceChangedHandler(context);
        var notification = new FoodItemPriceChanged(itemId: 1, oldPrice: 10.00m, newPrice: 12.00m);

        // Act & Assert - Should not throw
        await handler.Handle(notification, CancellationToken.None);
        
        // Verify no carts exist
        Assert.Empty(context.ShoppingCarts);
    }

    [Fact]
    public async Task Handle_ItemWithMultipleQuantity_UpdatesPriceForSingleItem()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m);
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m); // Increment count
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m); // Increment count again
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new FoodItemPriceChangedHandler(context);
        var notification = new FoodItemPriceChanged(itemId: 1, oldPrice: 10.00m, newPrice: 11.99m);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstAsync(c => c.Id == cart.Id);
        
        Assert.Single(updatedCart.Items);
        var item = updatedCart.Items.First();
        Assert.Equal(11.99m, item.Price);
        Assert.Equal(3, item.Count);
        Assert.Equal(35.97m, item.Sum); // 11.99 * 3
    }

    [Fact]
    public async Task Handle_PriceIncrease_UpdatesCorrectly()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new FoodItemPriceChangedHandler(context);
        var notification = new FoodItemPriceChanged(itemId: 1, oldPrice: 10.00m, newPrice: 15.00m);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstAsync(c => c.Id == cart.Id);
        
        var item = updatedCart.Items.First();
        Assert.Equal(15.00m, item.Price);
    }

    [Fact]
    public async Task Handle_PriceDecrease_UpdatesCorrectly()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 15.00m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new FoodItemPriceChangedHandler(context);
        var notification = new FoodItemPriceChanged(itemId: 1, oldPrice: 15.00m, newPrice: 9.99m);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstAsync(c => c.Id == cart.Id);
        
        var item = updatedCart.Items.First();
        Assert.Equal(9.99m, item.Price);
    }

    [Fact]
    public async Task Handle_NullContext_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FoodItemPriceChangedHandler(null!));
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new FoodItemPriceChangedHandler(context);
        var notification = new FoodItemPriceChanged(itemId: 1, oldPrice: 10.00m, newPrice: 12.00m);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(notification, cts.Token)
        );
    }

    [Fact]
    public async Task Handle_SavesChangesToDatabase()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new FoodItemPriceChangedHandler(context);
        var notification = new FoodItemPriceChanged(itemId: 1, oldPrice: 10.00m, newPrice: 13.50m);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert - Detach and reload from same context to verify persistence
        context.Entry(cart).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        
        var persistedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstAsync(c => c.Id == cart.Id);
        
        var persistedItem = persistedCart.Items.First();
        Assert.Equal(13.50m, persistedItem.Price);
    }

    [Fact]
    public async Task Handle_MultipleDifferentItemsInCart_UpdatesOnlyTargetItem()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m);
        cart.AddItem(itemId: 2, itemName: "Burger", itemPrice: 8.00m);
        cart.AddItem(itemId: 3, itemName: "Salad", itemPrice: 6.00m);
        cart.AddItem(itemId: 4, itemName: "Drink", itemPrice: 3.00m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new FoodItemPriceChangedHandler(context);
        var notification = new FoodItemPriceChanged(itemId: 3, oldPrice: 6.00m, newPrice: 7.50m);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstAsync(c => c.Id == cart.Id);
        
        Assert.Equal(10.00m, updatedCart.Items.First(i => i.Sku == 1).Price);
        Assert.Equal(8.00m, updatedCart.Items.First(i => i.Sku == 2).Price);
        Assert.Equal(7.50m, updatedCart.Items.First(i => i.Sku == 3).Price);
        Assert.Equal(3.00m, updatedCart.Items.First(i => i.Sku == 4).Price);
    }

    [Fact]
    public async Task Handle_UpdatesSum_WhenPriceChanges()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m);
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m); // Count = 2
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new FoodItemPriceChangedHandler(context);
        var notification = new FoodItemPriceChanged(itemId: 1, oldPrice: 10.00m, newPrice: 12.00m);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstAsync(c => c.Id == cart.Id);
        
        var item = updatedCart.Items.First();
        Assert.Equal(12.00m, item.Price);
        Assert.Equal(2, item.Count);
        Assert.Equal(24.00m, item.Sum); // 12.00 * 2
    }
}