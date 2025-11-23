using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Events;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Handlers;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Entities;
using TarlBreuJacoBaraKnor.webapp.Tests.Helpers;
using Xunit;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Ordering.Handlers;

public class OrderSentHandlerTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;

    public OrderSentHandlerTests(DbTest dbTest)
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
        // Arrange
        var mockNotificationService = new Mock<INotificationService>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new OrderSentHandler(null!, mockNotificationService.Object));
        Assert.Equal("db", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullNotificationService_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new OrderSentHandler(context, null!));
        Assert.Equal("notificationService", exception.ParamName);
    }

    [Fact]
    public async Task Handle_UpdatesOrderStatusToOnTheWay()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Being_picked_up);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderSentHandler(context, mockNotificationService.Object);
        var notification = new OrderSent(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        Assert.NotNull(updatedOrder);
        Assert.Equal(Status.On_the_way, updatedOrder.Status);
    }

    [Fact]
    public async Task Handle_SendsNotificationToCustomer()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Being_picked_up);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderSentHandler(context, mockNotificationService.Object);
        var notification = new OrderSent(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        mockNotificationService.Verify(
            service => service.SendOrderPickedUpNotification(customer.Id, order.Id),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithNonExistentOrderId_DoesNotUpdateOrNotify()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var handler = new OrderSentHandler(context, mockNotificationService.Object);
        var nonExistentOrderId = Guid.NewGuid();
        var notification = new OrderSent(nonExistentOrderId);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        mockNotificationService.Verify(
            service => service.SendOrderPickedUpNotification(It.IsAny<Guid>(), It.IsAny<Guid>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WithSubmittedOrder_UpdatesToOnTheWay()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Submitted);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderSentHandler(context, mockNotificationService.Object);
        var notification = new OrderSent(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        Assert.NotNull(updatedOrder);
        Assert.Equal(Status.On_the_way, updatedOrder.Status);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Being_picked_up);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderSentHandler(context, mockNotificationService.Object);
        var notification = new OrderSent(order.Id);
        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        await handler.Handle(notification, cancellationTokenSource.Token);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        Assert.NotNull(updatedOrder);
        Assert.Equal(Status.On_the_way, updatedOrder.Status);
    }

    [Fact]
    public async Task Handle_SavesChangesToDatabase()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Being_picked_up);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var originalStatus = order.Status;
        var handler = new OrderSentHandler(context, mockNotificationService.Object);
        var notification = new OrderSent(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        Assert.Equal(Status.Being_picked_up, originalStatus);
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        Assert.Equal(Status.On_the_way, updatedOrder!.Status);
    }

    [Fact]
    public async Task Handle_OnlyUpdatesSpecifiedOrder()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser();
        var order1 = CreateTestOrder(customer, Status.Being_picked_up);
        var order2 = CreateTestOrder(customer, Status.Being_picked_up);

        await context.Users.AddAsync(customer);
        await context.Orders.AddRangeAsync(order1, order2);
        await context.SaveChangesAsync();

        var handler = new OrderSentHandler(context, mockNotificationService.Object);
        var notification = new OrderSent(order1.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var updatedOrder1 = await context.Orders.FindAsync(order1.Id);
        var unchangedOrder2 = await context.Orders.FindAsync(order2.Id);
        
        Assert.Equal(Status.On_the_way, updatedOrder1!.Status);
        Assert.Equal(Status.Being_picked_up, unchangedOrder2!.Status);
    }

    [Fact]
    public async Task Handle_PreservesOtherOrderProperties()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser("John Doe", "john@example.com");
        var order = CreateTestOrder(customer, Status.Being_picked_up);
        
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

        var originalNotes = order.Notes;
        var originalOrderLineCount = order.OrderLines.Count;

        var handler = new OrderSentHandler(context, mockNotificationService.Object);
        var notification = new OrderSent(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        Assert.NotNull(updatedOrder);
        Assert.Equal(Status.On_the_way, updatedOrder.Status);
        Assert.Equal(originalNotes, updatedOrder.Notes);
        Assert.Equal(originalOrderLineCount, updatedOrder.OrderLines.Count);
    }

    [Fact]
    public async Task Handle_CanBeCalledMultipleTimes()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser();
        var order1 = CreateTestOrder(customer, Status.Being_picked_up);
        var order2 = CreateTestOrder(customer, Status.Being_picked_up);

        await context.Users.AddAsync(customer);
        await context.Orders.AddRangeAsync(order1, order2);
        await context.SaveChangesAsync();

        var handler = new OrderSentHandler(context, mockNotificationService.Object);

        // Act - Handle first order
        var notification1 = new OrderSent(order1.Id);
        await handler.Handle(notification1, CancellationToken.None);

        // Act - Handle second order
        var notification2 = new OrderSent(order2.Id);
        await handler.Handle(notification2, CancellationToken.None);

        // Assert
        var updatedOrder1 = await context.Orders.FindAsync(order1.Id);
        var updatedOrder2 = await context.Orders.FindAsync(order2.Id);
        
        Assert.Equal(Status.On_the_way, updatedOrder1!.Status);
        Assert.Equal(Status.On_the_way, updatedOrder2!.Status);

        mockNotificationService.Verify(
            service => service.SendOrderPickedUpNotification(customer.Id, order1.Id),
            Times.Once
        );
        mockNotificationService.Verify(
            service => service.SendOrderPickedUpNotification(customer.Id, order2.Id),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithAlreadyOnTheWayOrder_RemainsOnTheWay()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.On_the_way);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderSentHandler(context, mockNotificationService.Object);
        var notification = new OrderSent(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        Assert.NotNull(updatedOrder);
        Assert.Equal(Status.On_the_way, updatedOrder.Status);
    }

    [Fact]
    public async Task Handle_WithDeliveredOrder_ChangesToOnTheWay()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Delivered);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderSentHandler(context, mockNotificationService.Object);
        var notification = new OrderSent(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        Assert.NotNull(updatedOrder);
        Assert.Equal(Status.On_the_way, updatedOrder.Status);
    }
}