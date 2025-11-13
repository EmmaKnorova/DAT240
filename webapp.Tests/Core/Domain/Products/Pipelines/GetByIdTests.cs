using System;
using System.Threading;
using System.Threading.Tasks;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Exceptions;
using TarlBreuJacoBaraKnor.webapp.Tests.Helpers;
using Xunit;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Products.Pipelines;

public class GetByIdTests : DbTest
{
    [Fact]
    public async Task Handle_WhenFoodItemExists_ReturnsFoodItem()
    {
        // Arrange
        using var context = CreateContext();
        
        var foodItem = new FoodItem("Pizza", "Delicious pizza") { Price = 99.99m };
        context.FoodItems.Add(foodItem);
        await context.SaveChangesAsync();

        var handler = new GetById.Handler(context);
        var request = new GetById.Request(foodItem.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(foodItem.Id, result.Id);
        Assert.Equal("Pizza", result.Name);
        Assert.Equal(99.99m, result.Price);
    }

    [Fact]
    public async Task Handle_WhenFoodItemDoesNotExist_ThrowsEntityNotFoundException()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new GetById.Handler(context);
        var request = new GetById.Request(999);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(request, CancellationToken.None)
        );
        
        Assert.Contains("FoodItem with Id 999 was not found", exception.Message);
    }

    [Fact]
    public async Task Handle_WithMultipleItems_ReturnsCorrectItem()
    {
        // Arrange
        using var context = CreateContext();
        
        context.FoodItems.Add(new FoodItem("Pizza", "Delicious pizza") { Price = 99.99m });
        context.FoodItems.Add(new FoodItem("Burger", "Tasty burger") { Price = 79.99m });
        var targetItem = new FoodItem("Salad", "Fresh salad") { Price = 59.99m };
        context.FoodItems.Add(targetItem);
        await context.SaveChangesAsync();

        var handler = new GetById.Handler(context);
        var request = new GetById.Request(targetItem.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(targetItem.Id, result.Id);
        Assert.Equal("Salad", result.Name);
        Assert.Equal(59.99m, result.Price);
    }
}