namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;

public interface INotificationService
{
    Task SendOrderStatusNotification(Guid userId, Guid orderId, string status, string message);
    Task SendOrderAcceptedNotification(Guid customerId, Guid orderId);
    Task SendOrderPickedUpNotification(Guid customerId, Guid orderId);
    Task SendOrderDeliveredNotification(Guid customerId, Guid orderId);
    Task SendNewOrderNotification(Guid courierId, Guid orderId);
}