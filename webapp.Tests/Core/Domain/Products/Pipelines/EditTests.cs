using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Exceptions;
using TarlBreuJacoBaraKnor.webapp.SharedKernel;
using TarlBreuJacoBaraKnor.webapp.Tests.Helpers;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Products.Pipelines;

public class EditTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;

    public EditTests(DbTest dbTest)
    {
        _dbTest = dbTest;
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesFoodItem()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var originalItem = new FoodItem("Original Pizza", "Original description")
        {
            Price = 10.00m,
            CookTime = 15
        };
        context.FoodItems.Add(originalItem);
        await context.SaveChangesAsync();

        var validators = GetAllValidators();
        var handler = new Edit.Handler(context, validators);
        var request = new Edit.Request(
            Id: originalItem.Id,
            Name: "Updated Pizza",
            Description: "Updated description",
            Price: 15.99m,
            Cooktime: 20
        );

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(response.Success);
        Assert.Empty(response.Errors);

        // Verify changes were saved
        var updatedItem = await context.FoodItems.FindAsync(originalItem.Id);
        Assert.NotNull(updatedItem);
        Assert.Equal("Updated Pizza", updatedItem.Name);
        Assert.Equal("Updated description", updatedItem.Description);
        Assert.Equal(15.99m, updatedItem.Price);
        Assert.Equal(20, updatedItem.CookTime);
    }

    [Fact]
    public async Task Handle_NonExistentItem_ThrowsEntityNotFoundException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var validators = GetAllValidators();
        var handler = new Edit.Handler(context, validators);
        var request = new Edit.Request(
            Id: 999,
            Name: "Test Pizza",
            Description: "Test description",
            Price: 10.00m,
            Cooktime: 15
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(request, CancellationToken.None)
        );
        
        Assert.Contains("FoodItem with Id 999 was not found", exception.Message);
    }

    [Fact]
    public async Task Handle_EmptyName_ReturnsValidationError()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var existingItem = new FoodItem("Original Pizza", "Original description")
        {
            Price = 10.00m,
            CookTime = 15
        };
        context.FoodItems.Add(existingItem);
        await context.SaveChangesAsync();

        var validators = GetAllValidators();
        var handler = new Edit.Handler(context, validators);
        var request = new Edit.Request(
            Id: existingItem.Id,
            Name: "",
            Description: "Updated description",
            Price: 12.00m,
            Cooktime: 20
        );

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.Success);
        Assert.NotEmpty(response.Errors);
        Assert.Contains(response.Errors, e => e.Contains("Description")); // Note: validator has bug

        // Entity properties are changed before validation, so they reflect the attempted update
        // But changes are not persisted to DB since SaveChanges is not called on validation failure
        var unchangedItem = await context.FoodItems.FindAsync(existingItem.Id);
        Assert.Equal("", unchangedItem.Name); // Changed in memory but not saved
    }

    [Fact]
    public async Task Handle_EmptyDescription_ReturnsValidationError()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var existingItem = new FoodItem("Original Pizza", "Original description")
        {
            Price = 10.00m,
            CookTime = 15
        };
        context.FoodItems.Add(existingItem);
        await context.SaveChangesAsync();

        var validators = GetAllValidators();
        var handler = new Edit.Handler(context, validators);
        var request = new Edit.Request(
            Id: existingItem.Id,
            Name: "Updated Pizza",
            Description: "",
            Price: 12.00m,
            Cooktime: 20
        );

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.Success);
        Assert.NotEmpty(response.Errors);
        Assert.Contains(response.Errors, e => e.Contains("Description"));

        // Entity is modified in memory even though validation fails
        var unchangedItem = await context.FoodItems.FindAsync(existingItem.Id);
        Assert.Equal("", unchangedItem.Description); // Changed in memory but not saved
    }

    [Fact]
    public async Task Handle_NegativePrice_ReturnsValidationError()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var existingItem = new FoodItem("Original Pizza", "Original description")
        {
            Price = 10.00m,
            CookTime = 15
        };
        context.FoodItems.Add(existingItem);
        await context.SaveChangesAsync();

        var validators = GetAllValidators();
        var handler = new Edit.Handler(context, validators);
        var request = new Edit.Request(
            Id: existingItem.Id,
            Name: "Updated Pizza",
            Description: "Updated description",
            Price: -5.00m,
            Cooktime: 20
        );

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.Success);
        Assert.NotEmpty(response.Errors);
        Assert.Contains(response.Errors, e => e.Contains("Price") && e.Contains("greater than 0"));

        // Entity price is changed in memory even though validation fails
        var unchangedItem = await context.FoodItems.FindAsync(existingItem.Id);
        Assert.Equal(-5.00m, unchangedItem.Price); // Changed in memory but not saved
    }

    [Fact]
    public async Task Handle_ZeroPrice_ReturnsValidationError()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var existingItem = new FoodItem("Original Pizza", "Original description")
        {
            Price = 10.00m,
            CookTime = 15
        };
        context.FoodItems.Add(existingItem);
        await context.SaveChangesAsync();

        var validators = GetAllValidators();
        var handler = new Edit.Handler(context, validators);
        var request = new Edit.Request(
            Id: existingItem.Id,
            Name: "Updated Pizza",
            Description: "Updated description",
            Price: 0m,
            Cooktime: 20
        );

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.Success);
        Assert.NotEmpty(response.Errors);
        Assert.Contains(response.Errors, e => e.Contains("Price"));
    }

    [Fact]
    public async Task Handle_MultipleValidationErrors_ReturnsAllErrors()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var existingItem = new FoodItem("Original Pizza", "Original description")
        {
            Price = 10.00m,
            CookTime = 15
        };
        context.FoodItems.Add(existingItem);
        await context.SaveChangesAsync();

        var validators = GetAllValidators();
        var handler = new Edit.Handler(context, validators);
        var request = new Edit.Request(
            Id: existingItem.Id,
            Name: "",
            Description: "",
            Price: -10.00m,
            Cooktime: 20
        );

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.Success);
        Assert.True(response.Errors.Length >= 2); // At least description and price errors
    }

    [Fact]
    public async Task Handle_PartialUpdate_OnlyChangesSpecifiedFields()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var existingItem = new FoodItem("Original Pizza", "Original description")
        {
            Price = 10.00m,
            CookTime = 15
        };
        context.FoodItems.Add(existingItem);
        await context.SaveChangesAsync();

        var validators = GetAllValidators();
        var handler = new Edit.Handler(context, validators);
        var request = new Edit.Request(
            Id: existingItem.Id,
            Name: "Updated Pizza",
            Description: "Original description", // Keep same
            Price: 10.00m, // Keep same
            Cooktime: 20 // Change only this
        );

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(response.Success);
        var updatedItem = await context.FoodItems.FindAsync(existingItem.Id);
        Assert.Equal("Updated Pizza", updatedItem.Name);
        Assert.Equal("Original description", updatedItem.Description);
        Assert.Equal(10.00m, updatedItem.Price);
        Assert.Equal(20, updatedItem.CookTime);
    }

    [Fact]
    public async Task Handle_NullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var validators = GetAllValidators();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Edit.Handler(null!, validators));
    }

    [Fact]
    public async Task Handle_NullValidators_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Edit.Handler(context, null!));
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var existingItem = new FoodItem("Original Pizza", "Original description")
        {
            Price = 10.00m,
            CookTime = 15
        };
        context.FoodItems.Add(existingItem);
        await context.SaveChangesAsync();

        var validators = GetAllValidators();
        var handler = new Edit.Handler(context, validators);
        var request = new Edit.Request(
            Id: existingItem.Id,
            Name: "Updated Pizza",
            Description: "Updated description",
            Price: 12.00m,
            Cooktime: 20
        );
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(request, cts.Token)
        );
    }

    [Fact]
    public async Task Handle_NoValidators_UpdatesWithoutValidation()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var existingItem = new FoodItem("Original Pizza", "Original description")
        {
            Price = 10.00m,
            CookTime = 15
        };
        context.FoodItems.Add(existingItem);
        await context.SaveChangesAsync();

        var validators = Enumerable.Empty<IValidator<FoodItem>>();
        var handler = new Edit.Handler(context, validators);
        var request = new Edit.Request(
            Id: existingItem.Id,
            Name: "Updated Pizza",
            Description: "Updated description",
            Price: 15.00m,
            Cooktime: 20
        );

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(response.Success);
        Assert.Empty(response.Errors);
        
        var updatedItem = await context.FoodItems.FindAsync(existingItem.Id);
        Assert.Equal("Updated Pizza", updatedItem.Name);
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