using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Tests.Helpers;
using Xunit;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Products.Pipelines;

public class GetTests : DbTest
{
    [Fact]
    public async Task Handle_WhenNoFoodItems_ReturnsEmptyList()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new Get.Handler(context);
        var request = new Get.Request();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WhenFoodItemsExist_ReturnsAllItems()
    {
        // Arrange
        using var context = CreateContext();
        
        var foodItems = new List<FoodItem>
        {
            new FoodItem("Pizza", "Delicious pizza") { Price = 99.99m },
            new FoodItem("Burger", "Tasty burger") { Price = 79.99m },
            new FoodItem("Salad", "Fresh salad") { Price = 59.99m }
        };
        
        context.FoodItems.AddRange(foodItems);
        await context.SaveChangesAsync();

        var handler = new Get.Handler(context);
        var request = new Get.Request();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Handle_ReturnsFoodItemsOrderedByName()
    {
        // Arrange
        using var context = CreateContext();
        
        var foodItems = new List<FoodItem>
        {
            new FoodItem("Zebra Cake", "Sweet cake") { Price = 49.99m },
            new FoodItem("Apple Pie", "Classic pie") { Price = 39.99m },
            new FoodItem("Mango Smoothie", "Refreshing drink") { Price = 29.99m }
        };
        
        context.FoodItems.AddRange(foodItems);
        await context.SaveChangesAsync();

        var handler = new Get.Handler(context);
        var request = new Get.Request();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal("Apple Pie", result[0].Name);
        Assert.Equal("Mango Smoothie", result[1].Name);
        Assert.Equal("Zebra Cake", result[2].Name);
    }

    [Fact]
    public async Task Handle_WithSingleItem_ReturnsSingleItemList()
    {
        // Arrange
        using var context = CreateContext();
        
        var foodItem = new FoodItem("Taco", "Mexican taco") { Price = 69.99m };
        context.FoodItems.Add(foodItem);
        await context.SaveChangesAsync();

        var handler = new Get.Handler(context);
        var request = new Get.Request();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("Taco", result.First().Name);
        Assert.Equal(69.99m, result.First().Price);
    }
}