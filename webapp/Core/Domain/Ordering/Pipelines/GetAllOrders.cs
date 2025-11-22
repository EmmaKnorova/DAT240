using MediatR;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;

public class GetAllOrders
{
    public record Request() : IRequest<List<Order>>;

    public class Handler : IRequestHandler<Request, List<Order>>
    {
        private readonly ShopContext _db;

        public Handler(ShopContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<List<Order>> Handle(Request request, CancellationToken cancellationToken)
        {
            return await _db.Orders
                .Include(o => o.OrderLines)
                .Include(o => o.Customer)
                .Include(o => o.Location)
                .OrderBy(o => o.OrderDate)
                .ToListAsync(cancellationToken);
        }
    }
}
