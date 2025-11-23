using MediatR;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Events;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Handlers;

public class OrderAcceptedHandler : INotificationHandler<OrderAccepted>
{
    private readonly ShopContext _db;
    private readonly INotificationService _notificationService;

    public OrderAcceptedHandler(ShopContext db, INotificationService notificationService)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task Handle(OrderAccepted notification, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(o => o.Customer)
            .SingleOrDefaultAsync(or => or.Id == notification.orderId, cancellationToken);

        if (order == null)
        {
            return;
        }

        await _notificationService.SendOrderAcceptedNotification(
            order.Customer.Id,
            order.Id
        );
    }
}