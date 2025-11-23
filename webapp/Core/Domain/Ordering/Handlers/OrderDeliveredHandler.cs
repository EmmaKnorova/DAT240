using MediatR;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Events;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Handlers;

public class OrderDeliveredHandler : INotificationHandler<OrderDelivered>
{
    private readonly ShopContext _db;
    private readonly INotificationService _notificationService;

    public OrderDeliveredHandler(ShopContext db, INotificationService notificationService)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task Handle(OrderDelivered notification, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(o => o.Customer)
            .SingleOrDefaultAsync(or => or.Id == notification.orderId, cancellationToken);

        if (order == null)
        {
            return;
        }

        await _notificationService.SendOrderDeliveredNotification(
            order.Customer.Id,
            order.Id
        );
    }
}