using MediatR;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Handlers;

public class OrderSentHandler : INotificationHandler<OrderSent>
{
   private readonly ShopContext _db;

	public OrderSentHandler(ShopContext db)
		=> _db = db ?? throw new ArgumentNullException(nameof(db));

	public async Task Handle(OrderSent notification, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.SingleOrDefaultAsync(or => or.Id == notification.orderId, cancellationToken);
        order.Status = Status.On_the_way;
		await _db.SaveChangesAsync(cancellationToken);
	}
}