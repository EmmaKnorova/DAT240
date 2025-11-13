using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;
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
        var handler = new AddItem.Handler(context);
        var cartId = Guid.NewGuid();
        var request = new AddItem.Request(
            ItemId: 1,
            ItemName: "Pizza",
            ItemPrice: 12.99m,
            CartId: cartId
        );

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        var cart = await context.Set<ShoppingCart>()
            .Include(c => c.Items)
            .SingleOrDefaultAsync(c => c.Id == cartId);

        Assert.NotNull(cart);
        Assert.Single(cart.Items);
        Assert.Equal("Pizza", cart.Items.First().Name);
        Assert.Equal(12.99m, cart.Items.First().Price);
    }

    [Fact]
    public async Task Handle_AddsItemToExistingCart_WhenCartExists()
    {
        // Arrange
        await using var context = _dbTest.CreateContext();
        var cartId = Guid.NewGuid();

        // Create existing cart
        var existingCart = new ShoppingCart(cartId);
        existingCart.AddItem(1, "Burger", 8.99m);
        context.Set<ShoppingCart>().Add(existingCart);
        await context.SaveChangesAsync();

        var handler = new AddItem.Handler(context);
        var request = new AddItem.Request(
            ItemId: 2,
            ItemName: "Fries",
            ItemPrice: 3.99m,
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
        var cartId = Guid.NewGuid();
        var request = new AddItem.Request(1, "Test", 1m, cartId);

        // Act - Add same item twice
        await using var setupContext = _dbTest.CreateContext();
        var handler = new AddItem.Handler(setupContext);
        
        await handler.Handle(request, CancellationToken.None);
        await handler.Handle(request, CancellationToken.None);

        // Assert
        var cart = await setupContext.Set<ShoppingCart>()
            .Include(c => c.Items)
            .SingleOrDefaultAsync(c => c.Id == cartId);

        Assert.NotNull(cart);
        Assert.Single(cart.Items); // Only 1 unique item
        Assert.Equal(2, cart.Items.First().Count); // Count is 2
    }
}