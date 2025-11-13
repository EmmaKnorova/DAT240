using MediatR;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;

public class Get
{
	public record Request : IRequest<List<Order>> { }

	public class Handler : IRequestHandler<Request, List<Order>>
	{
		private readonly ShopContext _db;

		public Handler(ShopContext db)
		{
			_db = db ?? throw new ArgumentNullException(nameof(db));
		}

		public async Task<List<Order>> Handle(Request request, CancellationToken cancellationToken)
			=> await _db.Orders.Include(or=>or.OrderLines).Include(or=>or.Customer).Include(or=>or.Location).OrderBy(i => i.Id).ToListAsync(cancellationToken: cancellationToken);
	}
}
