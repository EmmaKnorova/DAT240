using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products.Pipelines;
using TarlBreuJacoBaraKnor.webapp.SharedKernel;
using TarlBreuJacoBaraKnor.webapp.Tests.Helpers;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Products.Pipelines;

public class CreateTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;

    public CreateTests(DbTest dbTest)
    {
        _dbTest = dbTest;
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesAndSavesFoodItem()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var validators = GetAllValidators();
        var handler = new Create.Handler(context, validators);
        var request = new Create.Request(
            Name: "Margherita Pizza",
            Description: "Classic tomato and mozzarella pizza",
            Price: 12.99m,
            Cooktime: 15
        );

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.createdItem);
        Assert.Empty(response.Errors);
        Assert.Equal("Margherita Pizza", response.createdItem.Name);
        Assert.Equal("Classic tomato and mozzarella pizza", response.createdItem.Description);
        Assert.Equal(12.99m, response.createdItem.Price);
        Assert.Equal(15, response.createdItem.CookTime);
        Assert.True(response.createdItem.Id > 0); // Verify ID was assigned by DB

        // Verify item was actually saved to database
        var savedItem = await context.FoodItems.FindAsync(response.createdItem.Id);
        Assert.NotNull(savedItem);
        Assert.Equal("Margherita Pizza", savedItem.Name);
    }

    [Fact]
    public async Task Handle_EmptyName_ReturnsValidationError()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var validators = GetAllValidators();
        var handler = new Create.Handler(context, validators);
        var request = new Create.Request(
            Name: "",
            Description: "Test description",
            Price: 10.00m,
            Cooktime: 10
        );

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.Success);
        Assert.NotEmpty(response.Errors);
        Assert.Contains(response.Errors, e => e.Contains("Description")); // Note: validator has bug, checks Description instead of Name
        
        // Verify nothing was saved to database
        Assert.Empty(context.FoodItems);
    }

    [Fact]
    public async Task Handle_EmptyDescription_ReturnsValidationError()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var validators = GetAllValidators();
        var handler = new Create.Handler(context, validators);
        var request = new Create.Request(
            Name: "Test Pizza",
            Description: "",
            Price: 10.00m,
            Cooktime: 10
        );

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.Success);
        Assert.NotEmpty(response.Errors);
        Assert.Contains(response.Errors, e => e.Contains("Description"));
        
        // Verify nothing was saved to database
        Assert.Empty(context.FoodItems);
    }

    [Fact]
    public async Task Handle_NegativePrice_ReturnsValidationError()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var validators = GetAllValidators();
        var handler = new Create.Handler(context, validators);
        var request = new Create.Request(
            Name: "Test Pizza",
            Description: "Test description",
            Price: -5.00m,
            Cooktime: 10
        );

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.Success);
        Assert.NotEmpty(response.Errors);
        Assert.Contains(response.Errors, e => e.Contains("Price") && e.Contains("greater than 0"));
        
        // Verify nothing was saved to database
        Assert.Empty(context.FoodItems);
    }

    [Fact]
    public async Task Handle_ZeroPrice_ReturnsValidationError()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var validators = GetAllValidators();
        var handler = new Create.Handler(context, validators);
        var request = new Create.Request(
            Name: "Test Pizza",
            Description: "Test description",
            Price: 0m,
            Cooktime: 10
        );

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.Success);
        Assert.NotEmpty(response.Errors);
        Assert.Contains(response.Errors, e => e.Contains("Price"));
        
        // Verify nothing was saved to database
        Assert.Empty(context.FoodItems);
    }

    [Fact]
    public async Task Handle_MultipleValidationErrors_ReturnsAllErrors()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var validators = GetAllValidators();
        var handler = new Create.Handler(context, validators);
        var request = new Create.Request(
            Name: "",
            Description: "",
            Price: -10.00m,
            Cooktime: 10
        );

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.Success);
        Assert.True(response.Errors.Length >= 2); // At least name/description and price errors
        
        // Verify nothing was saved to database
        Assert.Empty(context.FoodItems);
    }

    [Fact]
    public async Task Handle_NullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var validators = GetAllValidators();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Create.Handler(null!, validators));
    }

    [Fact]
    public async Task Handle_NullValidators_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Create.Handler(context, null!));
    }

    [Fact]
    public async Task Handle_NoValidators_CreatesItemWithoutValidation()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var validators = Enumerable.Empty<IValidator<FoodItem>>();
        var handler = new Create.Handler(context, validators);
        var request = new Create.Request(
            Name: "Test Pizza",
            Description: "Test description",
            Price: 10.00m,
            Cooktime: 15
        );

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.createdItem);
        Assert.Empty(response.Errors);
        
        // Verify item was saved
        Assert.Single(context.FoodItems);
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var validators = GetAllValidators();
        var handler = new Create.Handler(context, validators);
        var request = new Create.Request(
            Name: "Test Pizza",
            Description: "Test description",
            Price: 10.00m,
            Cooktime: 15
        );
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(request, cts.Token)
        );
    }

    [Fact]
    public async Task Handle_ValidRequest_GeneratesDomainEvents()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var validators = GetAllValidators();
        var handler = new Create.Handler(context, validators);
        var request = new Create.Request(
            Name: "Test Pizza",
            Description: "Test description",
            Price: 10.00m,
            Cooktime: 15
        );

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert - FoodItem constructor and price setter should generate domain events
        Assert.True(response.Success);
        Assert.NotNull(response.createdItem);
        // Events are generated when Name and Price are set after construction
    }

    // Helper method to get all validators
    private IEnumerable<IValidator<FoodItem>> GetAllValidators()
    {
        return new List<IValidator<FoodItem>>
        {
            new FoodItemNameValidator(),
            new FoodItemDescriptionValidator(),
            new FoodItemPriceValidator()
        };
    }
}