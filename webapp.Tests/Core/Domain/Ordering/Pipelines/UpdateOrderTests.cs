using System;
using System.Threading;
using System.Threading.Tasks;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.Core.Domain.Identity.Entities;
using TarlBreuJacoBaraKnor.webapp.Tests.Helpers;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Ordering.Pipelines;

public class UpdateOrderTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;

    public UpdateOrderTests(DbTest dbTest)
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
    public async Task Handle_UpdatesOrderStatus_Successfully()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Submitted);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new UpdateOrder.Handler(context);
        
        // Update status
        order.Status = Status.Being_picked_up;
        var request = new UpdateOrder.Request(order);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Errors);

        // Verify in database
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        Assert.NotNull(updatedOrder);
        Assert.Equal(Status.Being_picked_up, updatedOrder.Status);
    }

    [Fact]
    public async Task Handle_WhenOrderIsNull_ReturnsFailure()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var handler = new UpdateOrder.Handler(context);
        var request = new UpdateOrder.Request(null!);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Single(result.Errors);
        Assert.Equal("Order cannot be null.", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ReturnsFailure()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer);
        
        // Don't save the order to database

        var handler = new UpdateOrder.Handler(context);
        var request = new UpdateOrder.Request(order);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Single(result.Errors);
        Assert.Equal("Order not found.", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_UpdatesFromSubmittedToBeingPickedUp()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Submitted);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new UpdateOrder.Handler(context);
        order.Status = Status.Being_picked_up;
        var request = new UpdateOrder.Request(order);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        Assert.Equal(Status.Being_picked_up, updatedOrder!.Status);
    }

    [Fact]
    public async Task Handle_UpdatesFromBeingPickedUpToOnTheWay()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Being_picked_up);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new UpdateOrder.Handler(context);
        order.Status = Status.On_the_way;
        var request = new UpdateOrder.Request(order);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        Assert.Equal(Status.On_the_way, updatedOrder!.Status);
    }

    [Fact]
    public async Task Handle_UpdatesFromOnTheWayToDelivered()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.On_the_way);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new UpdateOrder.Handler(context);
        order.Status = Status.Delivered;
        var request = new UpdateOrder.Request(order);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        Assert.Equal(Status.Delivered, updatedOrder!.Status);
    }

    [Fact]
    public async Task Handle_UpdatesToCancelled()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Submitted);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new UpdateOrder.Handler(context);
        order.Status = Status.Cancelled;
        var request = new UpdateOrder.Request(order);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        Assert.Equal(Status.Cancelled, updatedOrder!.Status);
    }

    [Fact]
    public async Task Handle_DoesNotModifyOtherOrders()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser();
        var order1 = CreateTestOrder(customer, Status.Submitted);
        var order2 = CreateTestOrder(customer, Status.Submitted);

        await context.Users.AddAsync(customer);
        await context.Orders.AddRangeAsync(order1, order2);
        await context.SaveChangesAsync();

        var handler = new UpdateOrder.Handler(context);
        order1.Status = Status.Delivered;
        var request = new UpdateOrder.Request(order1);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        var updatedOrder1 = await context.Orders.FindAsync(order1.Id);
        var unchangedOrder2 = await context.Orders.FindAsync(order2.Id);
        
        Assert.Equal(Status.Delivered, updatedOrder1!.Status);
        Assert.Equal(Status.Submitted, unchangedOrder2!.Status); // Should remain unchanged
    }

    [Fact]
    public async Task Handle_PreservesOtherOrderProperties()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Submitted);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var originalId = order.Id;
        var originalNotes = order.Notes;

        var handler = new UpdateOrder.Handler(context);
        order.Status = Status.Delivered;
        var request = new UpdateOrder.Request(order);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        var updatedOrder = await context.Orders
            .Include(o => o.Location)
            .Include(o => o.Customer)
            .FirstAsync(o => o.Id == originalId);
        
        Assert.Equal(originalId, updatedOrder.Id);
        Assert.Equal(originalNotes, updatedOrder.Notes);
        Assert.NotNull(updatedOrder.Customer);
        Assert.Equal(customer.Id, updatedOrder.Customer.Id);
        Assert.Equal("Main Building", updatedOrder.Location.Building);
    }

    [Fact]
    public async Task Handle_WithOrderContainingOrderLines_UpdatesSuccessfully()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser();
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
        await context.SaveChangesAsync();

        var handler = new UpdateOrder.Handler(context);
        order.Status = Status.Delivered;
        var request = new UpdateOrder.Request(order);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        var updatedOrder = await context.Orders
            .Include(o => o.OrderLines)
            .FirstAsync(o => o.Id == order.Id);
        
        Assert.Equal(Status.Delivered, updatedOrder.Status);
        Assert.Single(updatedOrder.OrderLines);
        Assert.Equal("Pizza", updatedOrder.OrderLines.First().FoodItemName);
    }

    [Fact]
    public async Task Handle_MultipleUpdates_WorkCorrectly()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Submitted);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new UpdateOrder.Handler(context);

        // Act & Assert - First update
        order.Status = Status.Being_picked_up;
        var result1 = await handler.Handle(new UpdateOrder.Request(order), CancellationToken.None);
        Assert.True(result1.Success);
        
        var updated1 = await context.Orders.FindAsync(order.Id);
        Assert.Equal(Status.Being_picked_up, updated1!.Status);

        // Act & Assert - Second update
        order.Status = Status.On_the_way;
        var result2 = await handler.Handle(new UpdateOrder.Request(order), CancellationToken.None);
        Assert.True(result2.Success);
        
        var updated2 = await context.Orders.FindAsync(order.Id);
        Assert.Equal(Status.On_the_way, updated2!.Status);

        // Act & Assert - Third update
        order.Status = Status.Delivered;
        var result3 = await handler.Handle(new UpdateOrder.Request(order), CancellationToken.None);
        Assert.True(result3.Success);
        
        var updated3 = await context.Orders.FindAsync(order.Id);
        Assert.Equal(Status.Delivered, updated3!.Status);
    }

    [Fact]
    public void Handle_ThrowsArgumentNullException_WhenDbContextIsNull()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => new UpdateOrder.Handler(null!));
    }

    [Fact]
    public async Task Handle_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Submitted);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new UpdateOrder.Handler(context);
        order.Status = Status.Delivered;
        var request = new UpdateOrder.Request(order);
        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        var result = await handler.Handle(request, cancellationTokenSource.Token);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Errors);
    }
}