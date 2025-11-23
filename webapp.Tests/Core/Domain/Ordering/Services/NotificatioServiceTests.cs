using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Moq;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;
using TarlBreuJacoBaraKnor.webapp.Hubs;
using Xunit;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Ordering.Services;

public class NotificationServiceTests
{
    private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
    private readonly Mock<IHubClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly NotificationService _notificationService;

    public NotificationServiceTests()
    {
        _mockHubContext = new Mock<IHubContext<NotificationHub>>();
        _mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();

        _mockHubContext.Setup(hub => hub.Clients).Returns(_mockClients.Object);
        _mockClients.Setup(clients => clients.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);

        _notificationService = new NotificationService(_mockHubContext.Object);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenHubContextIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new NotificationService(null));
    }

    [Fact]
    public async Task SendOrderStatusNotification_SendsMessageToCorrectGroup()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var status = "TestStatus";
        var message = "Test message";
        var expectedGroupName = $"user_{userId}";

        // Act
        await _notificationService.SendOrderStatusNotification(userId, orderId, status, message);

        // Assert
        _mockClients.Verify(
            clients => clients.Group(expectedGroupName),
            Times.Once
        );

        _mockClientProxy.Verify(
            proxy => proxy.SendCoreAsync(
                "ReceiveOrderNotification",
                It.Is<object[]>(args => ValidateNotificationArgs(args, orderId, status, message)),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task SendOrderAcceptedNotification_SendsCorrectNotification()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var expectedStatus = "Accepted";
        var expectedMessage = "Your order has been accepted! A courier will pick it up soon.";

        // Act
        await _notificationService.SendOrderAcceptedNotification(customerId, orderId);

        // Assert
        _mockClientProxy.Verify(
            proxy => proxy.SendCoreAsync(
                "ReceiveOrderNotification",
                It.Is<object[]>(args => ValidateNotificationArgs(args, orderId, expectedStatus, expectedMessage)),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task SendOrderPickedUpNotification_SendsCorrectNotification()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var expectedStatus = "PickedUp";
        var expectedMessage = "Your order has been picked up by a courier and is on its way!";

        // Act
        await _notificationService.SendOrderPickedUpNotification(customerId, orderId);

        // Assert
        _mockClientProxy.Verify(
            proxy => proxy.SendCoreAsync(
                "ReceiveOrderNotification",
                It.Is<object[]>(args => ValidateNotificationArgs(args, orderId, expectedStatus, expectedMessage)),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task SendOrderDeliveredNotification_SendsCorrectNotification()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var expectedStatus = "Delivered";
        var expectedMessage = "Your order has been delivered! Enjoy your meal!";

        // Act
        await _notificationService.SendOrderDeliveredNotification(customerId, orderId);

        // Assert
        _mockClientProxy.Verify(
            proxy => proxy.SendCoreAsync(
                "ReceiveOrderNotification",
                It.Is<object[]>(args => ValidateNotificationArgs(args, orderId, expectedStatus, expectedMessage)),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task SendNewOrderNotification_SendsCorrectNotification()
    {
        // Arrange
        var courierId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var expectedStatus = "NewOrder";
        var expectedMessage = "A new order is ready for pickup! Tap to view details.";

        // Act
        await _notificationService.SendNewOrderNotification(courierId, orderId);

        // Assert
        _mockClientProxy.Verify(
            proxy => proxy.SendCoreAsync(
                "ReceiveOrderNotification",
                It.Is<object[]>(args => ValidateNotificationArgs(args, orderId, expectedStatus, expectedMessage)),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task SendOrderStatusNotification_IncludesTimestamp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var beforeCall = DateTime.UtcNow;

        // Act
        await _notificationService.SendOrderStatusNotification(userId, orderId, "Status", "Message");
        var afterCall = DateTime.UtcNow;

        // Assert
        _mockClientProxy.Verify(
            proxy => proxy.SendCoreAsync(
                "ReceiveOrderNotification",
                It.Is<object[]>(args => ValidateTimestamp(args, beforeCall, afterCall)),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task SendOrderStatusNotification_FormatsUserGroupCorrectly()
    {
        // Arrange
        var userId = Guid.Parse("12345678-1234-1234-1234-123456789abc");
        var orderId = Guid.NewGuid();

        // Act
        await _notificationService.SendOrderStatusNotification(userId, orderId, "Status", "Message");

        // Assert
        _mockClients.Verify(
            clients => clients.Group("user_12345678-1234-1234-1234-123456789abc"),
            Times.Once
        );
    }

    private bool ValidateNotificationArgs(object[] args, Guid orderId, string status, string message)
    {
        if (args.Length != 1)
            return false;

        var notification = args[0];
        var type = notification.GetType();

        var orderIdProp = type.GetProperty("orderId");
        var statusProp = type.GetProperty("status");
        var messageProp = type.GetProperty("message");

        if (orderIdProp == null || statusProp == null || messageProp == null)
            return false;

        var actualOrderId = orderIdProp.GetValue(notification)?.ToString();
        var actualStatus = statusProp.GetValue(notification)?.ToString();
        var actualMessage = messageProp.GetValue(notification)?.ToString();

        return actualOrderId == orderId.ToString() &&
               actualStatus == status &&
               actualMessage == message;
    }

    private bool ValidateTimestamp(object[] args, DateTime beforeCall, DateTime afterCall)
    {
        if (args.Length != 1)
            return false;

        var notification = args[0];
        var type = notification.GetType();

        var timestampProp = type.GetProperty("timestamp");
        if (timestampProp == null)
            return false;

        var timestamp = timestampProp.GetValue(notification);
        if (timestamp is not DateTime timestampValue)
            return false;

        return timestampValue >= beforeCall && timestampValue <= afterCall;
    }
}