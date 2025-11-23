using MediatR;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;

public class GetOrderStatusTotals
{
    public record StatusCountDto(string Status, int Count);

    public record Response(IReadOnlyCollection<StatusCountDto> OrdersByStatus);

    public record Request : IRequest<Response>;

    public class Handler : IRequestHandler<Request, Response>
    {
        private readonly ShopContext _db;

        public Handler(ShopContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var data = await _db.Orders
                .GroupBy(o => o.Status)
                .Select(g => new StatusCountDto(g.Key.ToString(), g.Count()))
                .ToListAsync(cancellationToken);

            return new Response(data);
        }
    }
}