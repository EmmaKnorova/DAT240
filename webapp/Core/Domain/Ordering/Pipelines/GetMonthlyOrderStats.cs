using MediatR;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;

public class GetMonthlyOrderStats
{
    public record Request(int Year, int Month) : IRequest<Response>;

    public record StatusCountDto(string Status, int Count);

    public record Response(
        int TotalOrders,
        decimal TotalRevenue,
        IReadOnlyCollection<StatusCountDto> OrdersByStatus
    );

    public class Handler : IRequestHandler<Request, Response>
    {
        private readonly ShopContext _db;

        public Handler(ShopContext db)
            => _db = db ?? throw new ArgumentNullException(nameof(db));

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var startOfMonth = new DateTimeOffset(request.Year, request.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var endOfMonth = startOfMonth.AddMonths(1);

            var monthlyOrders = await _db.Orders
                .Include(o => o.OrderLines)
                .Where(o => o.OrderDate >= startOfMonth && o.OrderDate < endOfMonth)
                .ToListAsync(cancellationToken);

            var totalOrders = monthlyOrders.Count;

            var totalRevenue = monthlyOrders.Sum(o =>
                o.OrderLines.Sum(ol => ol.Price * ol.Amount)
            );

            var ordersByStatus = monthlyOrders
                .GroupBy(o => o.Status.ToString())
                .Select(g => new StatusCountDto(g.Key, g.Count()))
                .ToList()
                .AsReadOnly();

            return new Response(totalOrders, totalRevenue, ordersByStatus);
        }
    }
}