using MediatR;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;

public class AddOrder
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

            var exists = await _db.Orders.AnyAsync(or => or.Id == request.order.Id);
            if (exists)
                return new Response(false, new[] { "Order with this Id already exists." });

            await _db.Orders.AddAsync(request.order, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return new Response(true, []);
        }
    }
}
