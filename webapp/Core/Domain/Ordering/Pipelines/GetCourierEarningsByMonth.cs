using MediatR;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;

public class GetCourierEarningsByMonth
{
    public record Request(Guid CourierId, int Year) : IRequest<List<MonthlyEarningsDto>>;

    public record MonthlyEarningsDto(int Month, int NumberOfOrders, decimal RevenueDelivery, decimal RevenueTips, decimal TotalRevenue);

    public class Handler(ShopContext db) : IRequestHandler<Request, List<MonthlyEarningsDto>>
    {
        private readonly ShopContext _db = db;

        public async Task<List<MonthlyEarningsDto>> Handle(Request request, CancellationToken cancellationToken)
        {
            var result = new List<MonthlyEarningsDto>();

            for (int month = 1; month <= 12; month++)
            {
                var start = new DateTimeOffset(request.Year, month, 1, 0, 0, 0, TimeSpan.Zero);
                var end = start.AddMonths(1);

                var orders = await _db.Orders
                    .Where(o => o.OrderDate >= start && o.OrderDate < end)
                    .Where(o => o.Courier.Id == request.CourierId)
                    .Where(o => o.Status == Status.Delivered)
                    .ToListAsync(cancellationToken);

                var RevenueDelivery = orders.Sum(o => o.DeliveryFee) * 0.8m;
                //TODO now set to 100, has to be update when tips are implemented
                var RevenueTips = 100;
                var TotalRevenue = RevenueDelivery + RevenueTips;

                result.Add(new MonthlyEarningsDto(month, orders.Count(), RevenueDelivery, RevenueTips, TotalRevenue));
            }

            return result;
        }
    }
}
