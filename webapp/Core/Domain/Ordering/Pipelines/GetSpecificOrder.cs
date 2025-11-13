using MediatR;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;

public class GetSpecificOrder
{
    public record Request(Guid orderId) : IRequest<Order> { }

    public class Handler : IRequestHandler<Request, Order>
    {
        private readonly ShopContext _db;

        public Handler(ShopContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<Order> Handle(Request request, CancellationToken cancellationToken)
        {
            Order? order = await _db.Orders
                .Include(or => or.OrderLines)
                .Include(or => or.Customer)
                .Include(or => or.Location)
                .SingleOrDefaultAsync(or => or.Id == request.orderId, cancellationToken);

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {request.orderId} not found.");
            }

            return order;
        }
    }
}
