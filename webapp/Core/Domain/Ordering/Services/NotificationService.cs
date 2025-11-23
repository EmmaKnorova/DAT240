using Microsoft.AspNetCore.SignalR;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;
using TarlBreuJacoBaraKnor.webapp.Hubs;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    }

    public async Task SendOrderStatusNotification(Guid userId, Guid orderId, string status, string message)
    {
        var groupName = $"user_{userId}";
        
        await _hubContext.Clients.Group(groupName).SendAsync("ReceiveOrderNotification", new
        {
            orderId = orderId.ToString(),
            status,
            message,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task SendOrderAcceptedNotification(Guid customerId, Guid orderId)
    {
        await SendOrderStatusNotification(
            customerId,
            orderId,
            "Accepted",
            "Your order has been accepted! A courier will pick it up soon."
        );
    }

    public async Task SendOrderPickedUpNotification(Guid customerId, Guid orderId)
    {
        await SendOrderStatusNotification(
            customerId,
            orderId,
            "PickedUp",
            "Your order has been picked up by a courier and is on its way!"
        );
    }

    public async Task SendOrderDeliveredNotification(Guid customerId, Guid orderId)
    {
        await SendOrderStatusNotification(
            customerId,
            orderId,
            "Delivered",
            "Your order has been delivered! Enjoy your meal!"
        );
    }

    public async Task SendNewOrderNotification(Guid courierId, Guid orderId)
    {
        await SendOrderStatusNotification(
            courierId,
            orderId,
            "NewOrder",
            "A new order is ready for pickup! Tap to view details."
        );
    }
}