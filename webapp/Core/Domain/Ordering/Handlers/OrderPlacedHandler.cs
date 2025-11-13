using MediatR;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Handlers;

public class OrderPlacedHandler : INotificationHandler<OrderPlaced>
{
   private readonly ShopContext _db;

	public OrderPlacedHandler(ShopContext db)
		=> _db = db ?? throw new System.ArgumentNullException(nameof(db));

	public async Task Handle(OrderPlaced notification, CancellationToken cancellationToken)
	{
		await _db.SaveChangesAsync(cancellationToken);
	}
}