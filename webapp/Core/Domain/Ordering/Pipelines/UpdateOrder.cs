using MediatR;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;

public class UpdateOrder
{
    public record Request(Order order) : IRequest<Response>;

    public record Response(bool Success, string[] Errors);

    public class Handler : IRequestHandler<Request, Response>
    {
        private readonly ShopContext _db;

        public Handler(ShopContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            if (request.order == null)
                return new Response(false, new[] { "Order cannot be null." });

            var existingOrder = await _db.Orders.FirstOrDefaultAsync(o => o.Id == request.order.Id, cancellationToken);
            if (existingOrder == null)
                return new Response(false, new[] { "Order not found." });

            existingOrder.Status = request.order.Status;

            _db.Orders.Update(existingOrder);
            await _db.SaveChangesAsync(cancellationToken);

            return new Response(true, []);
        }
    }
}