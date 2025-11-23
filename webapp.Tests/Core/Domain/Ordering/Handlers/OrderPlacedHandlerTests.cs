using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
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

public class OrderPlacedHandlerTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;

    public OrderPlacedHandlerTests(DbTest dbTest)
    {
        _dbTest = dbTest;
    }

    private User CreateTestUser(string name = "Test User", string email = "test@example.com", AccountStates accountState = AccountStates.Approved)
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
            AccountState = accountState
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
        var mockUserManager = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new OrderPlacedHandler(null!, mockNotificationService.Object, mockUserManager.Object));
        Assert.Equal("db", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullNotificationService_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockUserManager = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new OrderPlacedHandler(context, null!, mockUserManager.Object));
        Assert.Equal("notificationService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullUserManager_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new OrderPlacedHandler(context, mockNotificationService.Object, null!));
        Assert.Equal("userManager", exception.ParamName);
    }

    [Fact]
    public async Task Handle_NotifiesAllApprovedCouriers()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserManager = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        var courier1 = CreateTestUser("Courier 1", "courier1@example.com", AccountStates.Approved);
        var courier2 = CreateTestUser("Courier 2", "courier2@example.com", AccountStates.Approved);
        var courier3 = CreateTestUser("Courier 3", "courier3@example.com", AccountStates.Approved);

        var couriers = new List<User> { courier1, courier2, courier3 };
        mockUserManager.Setup(um => um.GetUsersInRoleAsync("Courier"))
            .ReturnsAsync(couriers);

        var customer = CreateTestUser("Customer", "customer@example.com");
        var order = CreateTestOrder(customer);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderPlacedHandler(context, mockNotificationService.Object, mockUserManager.Object);
        var notification = new OrderPlaced(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        mockNotificationService.Verify(
            service => service.SendNewOrderNotification(courier1.Id, order.Id),
            Times.Once
        );
        mockNotificationService.Verify(
            service => service.SendNewOrderNotification(courier2.Id, order.Id),
            Times.Once
        );
        mockNotificationService.Verify(
            service => service.SendNewOrderNotification(courier3.Id, order.Id),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_DoesNotNotifyPendingCouriers()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserManager = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        var approvedCourier = CreateTestUser("Approved Courier", "approved@example.com", AccountStates.Approved);
        var pendingCourier = CreateTestUser("Pending Courier", "pending@example.com", AccountStates.Pending);

        var couriers = new List<User> { approvedCourier, pendingCourier };
        mockUserManager.Setup(um => um.GetUsersInRoleAsync("Courier"))
            .ReturnsAsync(couriers);

        var customer = CreateTestUser("Customer", "customer@example.com");
        var order = CreateTestOrder(customer);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderPlacedHandler(context, mockNotificationService.Object, mockUserManager.Object);
        var notification = new OrderPlaced(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        mockNotificationService.Verify(
            service => service.SendNewOrderNotification(approvedCourier.Id, order.Id),
            Times.Once
        );
        mockNotificationService.Verify(
            service => service.SendNewOrderNotification(pendingCourier.Id, order.Id),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_DoesNotNotifyRejectedCouriers()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserManager = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        var approvedCourier = CreateTestUser("Approved Courier", "approved@example.com", AccountStates.Approved);
        var rejectedCourier = CreateTestUser("Rejected Courier", "rejected@example.com", AccountStates.Declined);

        var couriers = new List<User> { approvedCourier, rejectedCourier };
        mockUserManager.Setup(um => um.GetUsersInRoleAsync("Courier"))
            .ReturnsAsync(couriers);

        var customer = CreateTestUser("Customer", "customer@example.com");
        var order = CreateTestOrder(customer);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderPlacedHandler(context, mockNotificationService.Object, mockUserManager.Object);
        var notification = new OrderPlaced(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        mockNotificationService.Verify(
            service => service.SendNewOrderNotification(approvedCourier.Id, order.Id),
            Times.Once
        );
        mockNotificationService.Verify(
            service => service.SendNewOrderNotification(rejectedCourier.Id, order.Id),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WithNoCouriers_DoesNotSendNotifications()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserManager = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        mockUserManager.Setup(um => um.GetUsersInRoleAsync("Courier"))
            .ReturnsAsync(new List<User>());

        var customer = CreateTestUser("Customer", "customer@example.com");
        var order = CreateTestOrder(customer);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderPlacedHandler(context, mockNotificationService.Object, mockUserManager.Object);
        var notification = new OrderPlaced(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        mockNotificationService.Verify(
            service => service.SendNewOrderNotification(It.IsAny<Guid>(), It.IsAny<Guid>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WithOnlyPendingCouriers_DoesNotSendNotifications()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserManager = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        var pendingCourier1 = CreateTestUser("Pending 1", "pending1@example.com", AccountStates.Pending);
        var pendingCourier2 = CreateTestUser("Pending 2", "pending2@example.com", AccountStates.Pending);

        var couriers = new List<User> { pendingCourier1, pendingCourier2 };
        mockUserManager.Setup(um => um.GetUsersInRoleAsync("Courier"))
            .ReturnsAsync(couriers);

        var customer = CreateTestUser("Customer", "customer@example.com");
        var order = CreateTestOrder(customer);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderPlacedHandler(context, mockNotificationService.Object, mockUserManager.Object);
        var notification = new OrderPlaced(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        mockNotificationService.Verify(
            service => service.SendNewOrderNotification(It.IsAny<Guid>(), It.IsAny<Guid>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserManager = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        var courier = CreateTestUser("Courier", "courier@example.com", AccountStates.Approved);
        mockUserManager.Setup(um => um.GetUsersInRoleAsync("Courier"))
            .ReturnsAsync(new List<User> { courier });

        var customer = CreateTestUser("Customer", "customer@example.com");
        var order = CreateTestOrder(customer);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderPlacedHandler(context, mockNotificationService.Object, mockUserManager.Object);
        var notification = new OrderPlaced(order.Id);
        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        await handler.Handle(notification, cancellationTokenSource.Token);

        // Assert
        mockNotificationService.Verify(
            service => service.SendNewOrderNotification(courier.Id, order.Id),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CanBeCalledMultipleTimes()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserManager = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        var courier = CreateTestUser("Courier", "courier@example.com", AccountStates.Approved);
        mockUserManager.Setup(um => um.GetUsersInRoleAsync("Courier"))
            .ReturnsAsync(new List<User> { courier });

        var customer = CreateTestUser("Customer", "customer@example.com");
        var order1 = CreateTestOrder(customer);
        var order2 = CreateTestOrder(customer);

        await context.Users.AddAsync(customer);
        await context.Orders.AddRangeAsync(order1, order2);
        await context.SaveChangesAsync();

        var handler = new OrderPlacedHandler(context, mockNotificationService.Object, mockUserManager.Object);

        // Act - Handle first order
        var notification1 = new OrderPlaced(order1.Id);
        await handler.Handle(notification1, CancellationToken.None);

        // Act - Handle second order
        var notification2 = new OrderPlaced(order2.Id);
        await handler.Handle(notification2, CancellationToken.None);

        // Assert
        mockNotificationService.Verify(
            service => service.SendNewOrderNotification(courier.Id, order1.Id),
            Times.Once
        );
        mockNotificationService.Verify(
            service => service.SendNewOrderNotification(courier.Id, order2.Id),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_SendsCorrectOrderIdToEachCourier()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserManager = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        var courier1 = CreateTestUser("Courier 1", "courier1@example.com", AccountStates.Approved);
        var courier2 = CreateTestUser("Courier 2", "courier2@example.com", AccountStates.Approved);

        mockUserManager.Setup(um => um.GetUsersInRoleAsync("Courier"))
            .ReturnsAsync(new List<User> { courier1, courier2 });

        var customer = CreateTestUser("Customer", "customer@example.com");
        var order = CreateTestOrder(customer);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderPlacedHandler(context, mockNotificationService.Object, mockUserManager.Object);
        var notification = new OrderPlaced(order.Id);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert - Each courier gets notification with same order ID
        mockNotificationService.Verify(
            service => service.SendNewOrderNotification(
                It.IsAny<Guid>(), 
                It.Is<Guid>(id => id == order.Id)
            ),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task Handle_DoesNotThrowException()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserManager = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        var courier = CreateTestUser("Courier", "courier@example.com", AccountStates.Approved);
        mockUserManager.Setup(um => um.GetUsersInRoleAsync("Courier"))
            .ReturnsAsync(new List<User> { courier });

        var customer = CreateTestUser("Customer", "customer@example.com");
        var order = CreateTestOrder(customer);

        await context.Users.AddAsync(customer);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new OrderPlacedHandler(context, mockNotificationService.Object, mockUserManager.Object);
        var notification = new OrderPlaced(order.Id);

        // Act & Assert - Should not throw
        await handler.Handle(notification, CancellationToken.None);
    }
}