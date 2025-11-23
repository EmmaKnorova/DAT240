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

public class OrderAcceptedHandlerTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;

    public OrderAcceptedHandlerTests(DbTest dbTest)
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
            new OrderAcceptedHandler(null!, mockNotificationService.Object));
        Assert.Equal("db", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullNotificationService_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new OrderAcceptedHandler(context, null!));
        Assert.Equal("notificationService", exception.ParamName);
    }

    [Fact]
    public async Task Handle_SendsAcceptedNotificationToCustomer()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Submitted);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderAcceptedHandler(context, mockNotificationService.Object);
        var notification = new OrderAccepted(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        mockNotificationService.Verify(
            service => service.SendOrderAcceptedNotification(customer.Id, order.Id),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithNonExistentOrderId_DoesNotSendNotification()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var handler = new OrderAcceptedHandler(context, mockNotificationService.Object);
        var nonExistentOrderId = Guid.NewGuid();
        var notification = new OrderAccepted(nonExistentOrderId);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        mockNotificationService.Verify(
            service => service.SendOrderAcceptedNotification(It.IsAny<Guid>(), It.IsAny<Guid>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WithBeingPickedUpOrder_SendsNotification()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Being_picked_up);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderAcceptedHandler(context, mockNotificationService.Object);
        var notification = new OrderAccepted(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        mockNotificationService.Verify(
            service => service.SendOrderAcceptedNotification(customer.Id, order.Id),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithOnTheWayOrder_SendsNotification()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.On_the_way);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderAcceptedHandler(context, mockNotificationService.Object);
        var notification = new OrderAccepted(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        mockNotificationService.Verify(
            service => service.SendOrderAcceptedNotification(customer.Id, order.Id),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Submitted);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderAcceptedHandler(context, mockNotificationService.Object);
        var notification = new OrderAccepted(order.Id);
        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        await handler.Handle(notification, cancellationTokenSource.Token);

        // Assert
        mockNotificationService.Verify(
            service => service.SendOrderAcceptedNotification(customer.Id, order.Id),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_DoesNotModifyOrderStatus()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Submitted);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var originalStatus = order.Status;
        var handler = new OrderAcceptedHandler(context, mockNotificationService.Object);
        var notification = new OrderAccepted(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var unchangedOrder = await context.Orders.FindAsync(order.Id);
        Assert.NotNull(unchangedOrder);
        Assert.Equal(originalStatus, unchangedOrder.Status);
    }

    [Fact]
    public async Task Handle_SendsNotificationWithCorrectCustomerId()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser("Jane Doe", "jane@example.com");
        var order = CreateTestOrder(customer, Status.Submitted);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderAcceptedHandler(context, mockNotificationService.Object);
        var notification = new OrderAccepted(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        mockNotificationService.Verify(
            service => service.SendOrderAcceptedNotification(
                It.Is<Guid>(id => id == customer.Id),
                It.Is<Guid>(id => id == order.Id)
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CanBeCalledMultipleTimes()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer1 = CreateTestUser("Customer 1", "customer1@example.com");
        var customer2 = CreateTestUser("Customer 2", "customer2@example.com");
        var order1 = CreateTestOrder(customer1, Status.Submitted);
        var order2 = CreateTestOrder(customer2, Status.Submitted);

        await context.Users.AddRangeAsync(customer1, customer2);
        await context.Orders.AddRangeAsync(order1, order2);
        await context.SaveChangesAsync();

        var handler = new OrderAcceptedHandler(context, mockNotificationService.Object);

        // Act - Handle first order
        var notification1 = new OrderAccepted(order1.Id);
        await handler.Handle(notification1, CancellationToken.None);

        // Act - Handle second order
        var notification2 = new OrderAccepted(order2.Id);
        await handler.Handle(notification2, CancellationToken.None);

        // Assert
        mockNotificationService.Verify(
            service => service.SendOrderAcceptedNotification(customer1.Id, order1.Id),
            Times.Once
        );
        mockNotificationService.Verify(
            service => service.SendOrderAcceptedNotification(customer2.Id, order2.Id),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_OnlyNotifiesSpecifiedOrder()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser();
        var order1 = CreateTestOrder(customer, Status.Submitted);
        var order2 = CreateTestOrder(customer, Status.Submitted);

        await context.Users.AddAsync(customer);
        await context.Orders.AddRangeAsync(order1, order2);
        await context.SaveChangesAsync();

        var handler = new OrderAcceptedHandler(context, mockNotificationService.Object);
        var notification = new OrderAccepted(order1.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert - Only order1 should be notified
        mockNotificationService.Verify(
            service => service.SendOrderAcceptedNotification(customer.Id, order1.Id),
            Times.Once
        );
        mockNotificationService.Verify(
            service => service.SendOrderAcceptedNotification(customer.Id, order2.Id),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_DoesNotThrowException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Submitted);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderAcceptedHandler(context, mockNotificationService.Object);
        var notification = new OrderAccepted(order.Id);

        // Act & Assert - Should not throw
        await handler.Handle(notification, CancellationToken.None);
    }

    [Fact]
    public async Task Handle_IncludesCustomerInQuery()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser("John Smith", "john@example.com");
        var order = CreateTestOrder(customer, Status.Submitted);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderAcceptedHandler(context, mockNotificationService.Object);
        var notification = new OrderAccepted(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert - Verifies customer data was loaded
        mockNotificationService.Verify(
            service => service.SendOrderAcceptedNotification(customer.Id, order.Id),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithDeliveredOrder_SendsNotification()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Delivered);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderAcceptedHandler(context, mockNotificationService.Object);
        var notification = new OrderAccepted(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        mockNotificationService.Verify(
            service => service.SendOrderAcceptedNotification(customer.Id, order.Id),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_PreservesOrderProperties()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var customer = CreateTestUser();
        var order = CreateTestOrder(customer, Status.Submitted);
        
        var orderLine = new OrderLine(
            foodItemId: Guid.NewGuid(),
            foodItemName: "Burger",
            amount: 1,
            price: 9.99m
        );
        order.AddOrderLine(orderLine);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var originalNotes = order.Notes;
        var originalOrderLineCount = order.OrderLines.Count;

        var handler = new OrderAcceptedHandler(context, mockNotificationService.Object);
        var notification = new OrderAccepted(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var unchangedOrder = await context.Orders.FindAsync(order.Id);
        Assert.NotNull(unchangedOrder);
        Assert.Equal(originalNotes, unchangedOrder.Notes);
        Assert.Equal(originalOrderLineCount, unchangedOrder.OrderLines.Count);
    }
}