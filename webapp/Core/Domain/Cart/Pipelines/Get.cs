using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;

public class Get
{
	public record Request(Guid CartId) : IRequest<ShoppingCart?>;


	public class Handler : IRequestHandler<Request, ShoppingCart?>
	{
		private readonly ShopContext _db;

		public Handler(ShopContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

		public async Task<ShoppingCart?> Handle(Request request, CancellationToken cancellationToken)
			=> await _db.ShoppingCarts.Include(c => c.Items)
										.Where(c => c.Id == request.CartId)
										.SingleOrDefaultAsync(cancellationToken);

	}
}