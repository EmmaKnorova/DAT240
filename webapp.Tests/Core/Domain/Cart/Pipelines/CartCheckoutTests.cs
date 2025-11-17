using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Dto;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.SharedKernel;
using TarlBreuJacoBaraKnor.webapp.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Cart.Pipelines;

public class CartCheckoutTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;

    public CartCheckoutTests(DbTest dbTest)
    {
        _dbTest = dbTest;
    }

    [Fact]
    public async Task Handle_ValidRequest_ChecksOutCartSuccessfully()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            UserName = "test@example.com",
            Address = "123 Test St",
            City = "Test City",
            PostalCode = "12345"
        };
        context.Users.Add(user);

        var cartId = Guid.NewGuid();
        var cart = new ShoppingCart(cartId);
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m);
        cart.AddItem(itemId: 2, itemName: "Burger", itemPrice: 8.00m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var orderingServiceMock = new Mock<IOrderingService>();
        var expectedOrderId = Guid.NewGuid();
        orderingServiceMock
            .Setup(s => s.PlaceOrder(It.IsAny<Location>(), It.IsAny<User>(), It.IsAny<OrderLineDto[]>(), It.IsAny<string>()))
            .ReturnsAsync(expectedOrderId);

        var cartValidators = Enumerable.Empty<IValidator<ShoppingCart>>();
        var locationValidators = Enumerable.Empty<IValidator<Location>>();

        var handler = new CartCheckout.Handler(context, orderingServiceMock.Object, cartValidators, locationValidators);
        var location = new Location { Building = "Test Building", RoomNumber = "Room 101" };
        var request = new CartCheckout.Request(cartId, location, userId, "Test notes");

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(response.success);
        Assert.Equal(expectedOrderId, response.OrderId);
        Assert.Empty(response.Errors);

        // Verify cart was removed
        var deletedCart = await context.ShoppingCarts.FindAsync(cartId);
        Assert.Null(deletedCart);

        // Verify PlaceOrder was called with correct parameters
        orderingServiceMock.Verify(s => s.PlaceOrder(
            It.Is<Location>(l => l.Building == "Test Building" && l.RoomNumber == "Room 101"),
            It.Is<User>(u => u.Id == userId),
            It.Is<OrderLineDto[]>(lines => lines.Length == 2),
            "Test notes"
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_CartNotFound_ReturnsError()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            UserName = "test@example.com",
            Address = "123 Test St",
            City = "Test City",
            PostalCode = "12345"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var orderingServiceMock = new Mock<IOrderingService>();
        var cartValidators = Enumerable.Empty<IValidator<ShoppingCart>>();
        var locationValidators = Enumerable.Empty<IValidator<Location>>();

        var handler = new CartCheckout.Handler(context, orderingServiceMock.Object, cartValidators, locationValidators);
        var location = new Location { Building = "Test Building", RoomNumber = "Room 101" };
        var request = new CartCheckout.Request(Guid.NewGuid(), location, userId);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.success);
        Assert.Equal(Guid.Empty, response.OrderId);
        Assert.Single(response.Errors);
        Assert.Contains("Cart not found", response.Errors);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsError()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        
        var cartId = Guid.NewGuid();
        var cart = new ShoppingCart(cartId);
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var orderingServiceMock = new Mock<IOrderingService>();
        var cartValidators = Enumerable.Empty<IValidator<ShoppingCart>>();
        var locationValidators = Enumerable.Empty<IValidator<Location>>();

        var handler = new CartCheckout.Handler(context, orderingServiceMock.Object, cartValidators, locationValidators);
        var location = new Location { Building = "Test Building", RoomNumber = "Room 101" };
        var request = new CartCheckout.Request(cartId, location, Guid.NewGuid());

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.success);
        Assert.Equal(Guid.Empty, response.OrderId);
        Assert.Single(response.Errors);
        Assert.Contains("User not found", response.Errors);
    }

    [Fact]
    public async Task Handle_CartValidationFails_ReturnsErrors()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            UserName = "test@example.com",
            Address = "123 Test St",
            City = "Test City",
            PostalCode = "12345"
        };
        context.Users.Add(user);

        var cartId = Guid.NewGuid();
        var cart = new ShoppingCart(cartId);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var orderingServiceMock = new Mock<IOrderingService>();

        var cartValidatorMock = new Mock<IValidator<ShoppingCart>>();
        cartValidatorMock
            .Setup(v => v.IsValid(It.IsAny<ShoppingCart>()))
            .Returns((false, "Cart is empty"));
        var cartValidators = new[] { cartValidatorMock.Object };

        var locationValidators = Enumerable.Empty<IValidator<Location>>();

        var handler = new CartCheckout.Handler(context, orderingServiceMock.Object, cartValidators, locationValidators);
        var location = new Location { Building = "Test Building", RoomNumber = "Room 101" };
        var request = new CartCheckout.Request(cartId, location, userId);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.success);
        Assert.Equal(Guid.Empty, response.OrderId);
        Assert.Single(response.Errors);
        Assert.Contains("Cart is empty", response.Errors);
    }

    [Fact]
    public async Task Handle_NullLocation_ReturnsError()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            UserName = "test@example.com",
            Address = "123 Test St",
            City = "Test City",
            PostalCode = "12345"
        };
        context.Users.Add(user);

        var cartId = Guid.NewGuid();
        var cart = new ShoppingCart(cartId);
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var orderingServiceMock = new Mock<IOrderingService>();
        var cartValidators = Enumerable.Empty<IValidator<ShoppingCart>>();
        var locationValidators = Enumerable.Empty<IValidator<Location>>();

        var handler = new CartCheckout.Handler(context, orderingServiceMock.Object, cartValidators, locationValidators);
        var request = new CartCheckout.Request(cartId, null!, userId);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.success);
        Assert.Equal(Guid.Empty, response.OrderId);
        Assert.Single(response.Errors);
        Assert.Contains("Location cannot be null", response.Errors);
    }

    [Fact]
    public async Task Handle_LocationValidationFails_ReturnsErrors()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            UserName = "test@example.com",
            Address = "123 Test St",
            City = "Test City",
            PostalCode = "12345"
        };
        context.Users.Add(user);

        var cartId = Guid.NewGuid();
        var cart = new ShoppingCart(cartId);
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var orderingServiceMock = new Mock<IOrderingService>();
        var cartValidators = Enumerable.Empty<IValidator<ShoppingCart>>();

        var locationValidatorMock = new Mock<IValidator<Location>>();
        locationValidatorMock
            .Setup(v => v.IsValid(It.IsAny<Location>()))
            .Returns((false, "Invalid building"));
        var locationValidators = new[] { locationValidatorMock.Object };

        var handler = new CartCheckout.Handler(context, orderingServiceMock.Object, cartValidators, locationValidators);
        var location = new Location { Building = "", RoomNumber = "Room 101" };
        var request = new CartCheckout.Request(cartId, location, userId);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.success);
        Assert.Equal(Guid.Empty, response.OrderId);
        Assert.Single(response.Errors);
        Assert.Contains("Invalid building", response.Errors);
    }

    [Fact]
    public async Task Handle_MultipleValidationErrors_ReturnsAllErrors()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            UserName = "test@example.com",
            Address = "123 Test St",
            City = "Test City",
            PostalCode = "12345"
        };
        context.Users.Add(user);

        var cartId = Guid.NewGuid();
        var cart = new ShoppingCart(cartId);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var orderingServiceMock = new Mock<IOrderingService>();

        var cartValidatorMock = new Mock<IValidator<ShoppingCart>>();
        cartValidatorMock
            .Setup(v => v.IsValid(It.IsAny<ShoppingCart>()))
            .Returns((false, "Cart is empty"));
        var cartValidators = new[] { cartValidatorMock.Object };

        var locationValidatorMock = new Mock<IValidator<Location>>();
        locationValidatorMock
            .Setup(v => v.IsValid(It.IsAny<Location>()))
            .Returns((false, "Invalid location"));
        var locationValidators = new[] { locationValidatorMock.Object };

        var handler = new CartCheckout.Handler(context, orderingServiceMock.Object, cartValidators, locationValidators);
        var location = new Location { Building = "", RoomNumber = "" };
        var request = new CartCheckout.Request(cartId, location, userId);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.success);
        Assert.Equal(Guid.Empty, response.OrderId);
        Assert.Equal(2, response.Errors.Length);
        Assert.Contains("Cart is empty", response.Errors);
        Assert.Contains("Invalid location", response.Errors);
    }

    [Fact]
    public async Task Handle_WithNotes_PassesNotesToOrderingService()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            UserName = "test@example.com",
            Address = "123 Test St",
            City = "Test City",
            PostalCode = "12345"
        };
        context.Users.Add(user);

        var cartId = Guid.NewGuid();
        var cart = new ShoppingCart(cartId);
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var orderingServiceMock = new Mock<IOrderingService>();
        orderingServiceMock
            .Setup(s => s.PlaceOrder(It.IsAny<Location>(), It.IsAny<User>(), It.IsAny<OrderLineDto[]>(), It.IsAny<string>()))
            .ReturnsAsync(Guid.NewGuid());

        var cartValidators = Enumerable.Empty<IValidator<ShoppingCart>>();
        var locationValidators = Enumerable.Empty<IValidator<Location>>();

        var handler = new CartCheckout.Handler(context, orderingServiceMock.Object, cartValidators, locationValidators);
        var location = new Location { Building = "Test Building", RoomNumber = "Room 101", Notes = "Leave at door" };
        var request = new CartCheckout.Request(cartId, location, userId, "Please add extra cheese");

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        orderingServiceMock.Verify(s => s.PlaceOrder(
            It.IsAny<Location>(),
            It.IsAny<User>(),
            It.IsAny<OrderLineDto[]>(),
            "Please add extra cheese"
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_NullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var orderingServiceMock = new Mock<IOrderingService>();
        var cartValidators = Enumerable.Empty<IValidator<ShoppingCart>>();
        var locationValidators = Enumerable.Empty<IValidator<Location>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new CartCheckout.Handler(null!, orderingServiceMock.Object, cartValidators, locationValidators));
    }

    [Fact]
    public async Task Handle_NullOrderingService_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var cartValidators = Enumerable.Empty<IValidator<ShoppingCart>>();
        var locationValidators = Enumerable.Empty<IValidator<Location>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new CartCheckout.Handler(context, null!, cartValidators, locationValidators));
    }

    [Fact]
    public async Task Handle_NullCartValidators_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var orderingServiceMock = new Mock<IOrderingService>();
        var locationValidators = Enumerable.Empty<IValidator<Location>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new CartCheckout.Handler(context, orderingServiceMock.Object, null!, locationValidators));
    }

    [Fact]
    public async Task Handle_NullLocationValidators_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var orderingServiceMock = new Mock<IOrderingService>();
        var cartValidators = Enumerable.Empty<IValidator<ShoppingCart>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new CartCheckout.Handler(context, orderingServiceMock.Object, cartValidators, null!));
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            UserName = "test@example.com",
            Address = "123 Test St",
            City = "Test City",
            PostalCode = "12345"
        };
        context.Users.Add(user);

        var cartId = Guid.NewGuid();
        var cart = new ShoppingCart(cartId);
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 10.00m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var orderingServiceMock = new Mock<IOrderingService>();
        var cartValidators = Enumerable.Empty<IValidator<ShoppingCart>>();
        var locationValidators = Enumerable.Empty<IValidator<Location>>();

        var handler = new CartCheckout.Handler(context, orderingServiceMock.Object, cartValidators, locationValidators);
        var location = new Location { Building = "Test Building", RoomNumber = "Room 101" };
        var request = new CartCheckout.Request(cartId, location, userId);
        
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(request, cts.Token)
        );
    }

    [Fact]
    public async Task Handle_ConvertsCartItemsToOrderLines_Correctly()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            UserName = "test@example.com",
            Address = "123 Test St",
            City = "Test City",
            PostalCode = "12345"
        };
        context.Users.Add(user);

        var cartId = Guid.NewGuid();
        var cart = new ShoppingCart(cartId);
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 12.99m);
        cart.AddItem(itemId: 1, itemName: "Pizza", itemPrice: 12.99m); // Count = 2
        cart.AddItem(itemId: 2, itemName: "Burger", itemPrice: 8.50m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var orderingServiceMock = new Mock<IOrderingService>();
        orderingServiceMock
            .Setup(s => s.PlaceOrder(It.IsAny<Location>(), It.IsAny<User>(), It.IsAny<OrderLineDto[]>(), It.IsAny<string>()))
            .ReturnsAsync(Guid.NewGuid());

        var cartValidators = Enumerable.Empty<IValidator<ShoppingCart>>();
        var locationValidators = Enumerable.Empty<IValidator<Location>>();

        var handler = new CartCheckout.Handler(context, orderingServiceMock.Object, cartValidators, locationValidators);
        var location = new Location { Building = "Test Building", RoomNumber = "Room 101" };
        var request = new CartCheckout.Request(cartId, location, userId);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        orderingServiceMock.Verify(s => s.PlaceOrder(
            It.IsAny<Location>(),
            It.IsAny<User>(),
            It.Is<OrderLineDto[]>(lines => 
                lines.Length == 2 &&
                lines.Any(l => l.FoodItemId == 1 && l.FoodItemName == "Pizza" && l.Amount == 2 && l.Price == 12.99m) &&
                lines.Any(l => l.FoodItemId == 2 && l.FoodItemName == "Burger" && l.Amount == 1 && l.Price == 8.50m)
            ),
            It.IsAny<string>()
        ), Times.Once);
    }
}