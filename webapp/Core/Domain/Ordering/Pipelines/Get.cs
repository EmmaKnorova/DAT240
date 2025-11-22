using MediatR;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;

public class Get
{
    public record Request(Guid UserId) : IRequest<List<Order>>;

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
                .Include(o => o.Courier)
                .Where(o => o.Customer != null && o.Customer.Id == request.UserId || o.Courier.Id == request.UserId)
                .OrderBy(o => o.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
