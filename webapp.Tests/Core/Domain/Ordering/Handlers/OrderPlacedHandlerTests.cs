using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Handlers;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.Core.Domain.Identity.Entities;
using TarlBreuJacoBaraKnor.webapp.Tests.Helpers;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Ordering.Handlers;

public class OrderPlacedHandlerTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;

    public OrderPlacedHandlerTests(DbTest dbTest)
    {
        _dbTest = dbTest;
    }

    private User CreateTestUser(string name = "Test User", string email = "test@example.com")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            UserName = email.Split('@')[0],
            Address = "123 Test Street",
            City = "Test City",
            PostalCode = "12345",
            AccountState = AccountStates.Approved
        };
    }

    private Order CreateTestOrder(User customer, Status status = Status.Submitted)
    {
        var location = new Location
        {
            Building = "Main Building",
            RoomNumber = "101",
            Notes = "Test location"
        };

        return new Order(location, customer, "Test Order")
        {
            Status = status
        };
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new OrderPlacedHandler(null!));
        Assert.Equal("db", exception.ParamName);
    }

    [Fact]
    public async Task Handle_SavesChangesToDatabase()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        // Don't call SaveChangesAsync yet

        var handler = new OrderPlacedHandler(context);
        var notification = new OrderPlaced(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert - Verify changes were saved in the same context
        var savedOrder = await context.Orders.FindAsync(order.Id);
        Assert.NotNull(savedOrder);
        Assert.Equal(order.Id, savedOrder.Id);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);

        var handler = new OrderPlacedHandler(context);
        var notification = new OrderPlaced(order.Id);
        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        await handler.Handle(notification, cancellationTokenSource.Token);

        // Assert - Verify in the same context
        var savedOrder = await context.Orders.FindAsync(order.Id);
        Assert.NotNull(savedOrder);
    }

    [Fact]
    public async Task Handle_WithMultipleOrders_SavesAllChanges()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser();
        var order1 = CreateTestOrder(customer);
        var order2 = CreateTestOrder(customer);

        await context.Users.AddAsync(customer);
        await context.Orders.AddRangeAsync(order1, order2);

        var handler = new OrderPlacedHandler(context);
        var notification = new OrderPlaced(order1.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert - Both orders should be saved in the same context
        var savedOrder1 = await context.Orders.FindAsync(order1.Id);
        var savedOrder2 = await context.Orders.FindAsync(order2.Id);
        
        Assert.NotNull(savedOrder1);
        Assert.NotNull(savedOrder2);
    }

    [Fact]
    public async Task Handle_PreservesOrderDetails()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser("John Doe", "john@example.com");
        var order = CreateTestOrder(customer, Status.Submitted);
        
        var orderLine = new OrderLine(
            foodItemId: Guid.NewGuid(),
            foodItemName: "Pizza",
            amount: 2,
            price: 12.99m
        );
        order.AddOrderLine(orderLine);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);

        var handler = new OrderPlacedHandler(context);
        var notification = new OrderPlaced(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert - Verify in the same context
        var savedOrder = await context.Orders
            .Include(o => o.OrderLines)
            .Include(o => o.Location)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == order.Id);
        
        Assert.NotNull(savedOrder);
        Assert.Equal(Status.Submitted, savedOrder.Status);
        Assert.Equal("Test Order", savedOrder.Notes);
        Assert.Single(savedOrder.OrderLines);
        Assert.Equal("Pizza", savedOrder.OrderLines.First().FoodItemName);
        Assert.Equal(2, savedOrder.OrderLines.First().Amount);
        Assert.Equal(12.99m, savedOrder.OrderLines.First().Price);
        Assert.Equal("Main Building", savedOrder.Location.Building);
        Assert.Equal("John Doe", savedOrder.Customer.Name);
    }

    [Fact]
    public async Task Handle_WithOrderContainingMultipleOrderLines_SavesAll()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer);
        
        var orderLine1 = new OrderLine(Guid.NewGuid(), "Pizza", 2, 10.00m);
        var orderLine2 = new OrderLine(Guid.NewGuid(), "Burger", 1, 8.00m);
        var orderLine3 = new OrderLine(Guid.NewGuid(), "Soda", 3, 3.50m);
        
        order.AddOrderLine(orderLine1);
        order.AddOrderLine(orderLine2);
        order.AddOrderLine(orderLine3);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);

        var handler = new OrderPlacedHandler(context);
        var notification = new OrderPlaced(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var savedOrder = await context.Orders
            .Include(o => o.OrderLines)
            .FirstOrDefaultAsync(o => o.Id == order.Id);
        
        Assert.NotNull(savedOrder);
        Assert.Equal(3, savedOrder.OrderLines.Count);
        Assert.Contains(savedOrder.OrderLines, ol => ol.FoodItemName == "Pizza");
        Assert.Contains(savedOrder.OrderLines, ol => ol.FoodItemName == "Burger");
        Assert.Contains(savedOrder.OrderLines, ol => ol.FoodItemName == "Soda");
    }

    [Fact]
    public async Task Handle_CanBeCalledMultipleTimes()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser();
        var order1 = CreateTestOrder(customer);
        var order2 = CreateTestOrder(customer);

        var handler = new OrderPlacedHandler(context);

        // Act - Handle first order
        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order1);
        var notification1 = new OrderPlaced(order1.Id);
        await handler.Handle(notification1, CancellationToken.None);

        // Act - Handle second order
        await context.Orders.AddAsync(order2);
        var notification2 = new OrderPlaced(order2.Id);
        await handler.Handle(notification2, CancellationToken.None);

        // Assert
        var savedOrder1 = await context.Orders.FindAsync(order1.Id);
        var savedOrder2 = await context.Orders.FindAsync(order2.Id);
        
        Assert.NotNull(savedOrder1);
        Assert.NotNull(savedOrder2);
    }

    [Fact]
    public async Task Handle_WithDifferentOrderStatuses_SavesCorrectly()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser();
        var submittedOrder = CreateTestOrder(customer, Status.Submitted);
        var pickupOrder = CreateTestOrder(customer, Status.Being_picked_up);
        var deliveredOrder = CreateTestOrder(customer, Status.Delivered);

        await context.Users.AddAsync(customer);
        await context.Orders.AddRangeAsync(submittedOrder, pickupOrder, deliveredOrder);

        var handler = new OrderPlacedHandler(context);
        var notification = new OrderPlaced(submittedOrder.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var saved1 = await context.Orders.FindAsync(submittedOrder.Id);
        var saved2 = await context.Orders.FindAsync(pickupOrder.Id);
        var saved3 = await context.Orders.FindAsync(deliveredOrder.Id);
        
        Assert.NotNull(saved1);
        Assert.NotNull(saved2);
        Assert.NotNull(saved3);
        Assert.Equal(Status.Submitted, saved1.Status);
        Assert.Equal(Status.Being_picked_up, saved2.Status);
        Assert.Equal(Status.Delivered, saved3.Status);
    }

    [Fact]
    public async Task Handle_WithEmptyOrder_SavesSuccessfully()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer);
        // Order has no order lines

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);

        var handler = new OrderPlacedHandler(context);
        var notification = new OrderPlaced(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var savedOrder = await context.Orders
            .Include(o => o.OrderLines)
            .FirstOrDefaultAsync(o => o.Id == order.Id);
        
        Assert.NotNull(savedOrder);
        Assert.Empty(savedOrder.OrderLines);
    }

    [Fact]
    public async Task Handle_DoesNotThrowException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);

        var handler = new OrderPlacedHandler(context);
        var notification = new OrderPlaced(order.Id);

        // Act & Assert - Should not throw
        await handler.Handle(notification, CancellationToken.None);
    }
}