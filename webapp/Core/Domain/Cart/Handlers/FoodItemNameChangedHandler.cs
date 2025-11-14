using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products.Events;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Handlers;

public class FoodItemNameChangedHandler : INotificationHandler<FoodItemNameChanged>
{
	private readonly ShopContext _db;

	public FoodItemNameChangedHandler(ShopContext db)
		=> _db = db ?? throw new System.ArgumentNullException(nameof(db));

	public async Task Handle(FoodItemNameChanged notification, CancellationToken cancellationToken)
	{
		var carts = await _db.ShoppingCarts.Include(c => c.Items)
						.Where(c => c.Items.Any(i => i.Sku == notification.ItemId))
						.ToListAsync(cancellationToken);
		foreach (var cart in carts)
		{
			foreach (var item in cart.Items.Where(i => i.Sku == notification.ItemId))
			{
				item.Name = notification.NewName;
			}
		}
		await _db.SaveChangesAsync(cancellationToken);
	}
}
