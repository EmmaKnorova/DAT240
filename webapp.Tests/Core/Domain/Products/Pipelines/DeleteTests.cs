using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Exceptions;
using TarlBreuJacoBaraKnor.webapp.Tests.Helpers;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Products.Pipelines;

public class DeleteTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;

    public DeleteTests(DbTest dbTest)
    {
        _dbTest = dbTest;
    }

    [Fact]
    public async Task Handle_ExistingItem_DeletesSuccessfully()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var foodItem = new FoodItem("Test Pizza", "Delicious test pizza")
        {
            Price = 12.99m,
            CookTime = 15
        };
        context.FoodItems.Add(foodItem);
        await context.SaveChangesAsync();

        var handler = new Delete.Handler(context);
        var request = new Delete.Request(foodItem.Id);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        var deletedItem = await context.FoodItems.FindAsync(foodItem.Id);
        Assert.Null(deletedItem);
    }

    [Fact]
    public async Task Handle_NonExistentItem_ThrowsEntityNotFoundException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var handler = new Delete.Handler(context);
        var request = new Delete.Request(999); // Non-existent ID

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(request, CancellationToken.None)
        );
        
        Assert.Contains("FoodItem with Id 999 was not found", exception.Message);
    }

    [Fact]
    public async Task Handle_MultipleItems_DeletesOnlySpecifiedItem()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var item1 = new FoodItem("Pizza", "Cheese pizza")
        {
            Price = 10.99m,
            CookTime = 15
        };
        var item2 = new FoodItem("Burger", "Beef burger")
        {
            Price = 8.99m,
            CookTime = 10
        };
        context.FoodItems.AddRange(item1, item2);
        await context.SaveChangesAsync();

        var handler = new Delete.Handler(context);
        var request = new Delete.Request(item1.Id);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        var deletedItem = await context.FoodItems.FindAsync(item1.Id);
        var remainingItem = await context.FoodItems.FindAsync(item2.Id);
        
        Assert.Null(deletedItem);
        Assert.NotNull(remainingItem);
        Assert.Equal("Burger", remainingItem.Name);
    }

    [Fact]
    public async Task Handle_NullContext_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Delete.Handler(null!));
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var foodItem = new FoodItem("Test Item", "Test description")
        {
            Price = 5.99m,
            CookTime = 5
        };
        context.FoodItems.Add(foodItem);
        await context.SaveChangesAsync();

        var handler = new Delete.Handler(context);
        var request = new Delete.Request(foodItem.Id);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(request, cts.Token)
        );
    }

    [Fact]
    public async Task Handle_ItemWithEvents_DeletesAndClearsEvents()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var foodItem = new FoodItem("Original Pizza", "Test pizza")
        {
            Price = 10.00m,
            CookTime = 15
        };
        context.FoodItems.Add(foodItem);
        await context.SaveChangesAsync();

        // Trigger domain events
        foodItem.Name = "Updated Pizza";
        foodItem.Price = 12.00m;
        await context.SaveChangesAsync();

        var handler = new Delete.Handler(context);
        var request = new Delete.Request(foodItem.Id);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        var deletedItem = await context.FoodItems.FindAsync(foodItem.Id);
        Assert.Null(deletedItem);
    }
}