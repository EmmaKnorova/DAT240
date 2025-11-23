using MediatR;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;
using webapp.Core.Domain.Ordering.Pipelines;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;

public class GetCourierEarningsByMonth
{
    public record Request(Guid CourierId, int Year) : IRequest<List<MonthlyEarningsDto>>;

    public record MonthlyEarningsDto(int Month, int NumberOfOrders, decimal RevenueDelivery, decimal RevenueTips, decimal TotalRevenue);

    public class Handler(ShopContext db, IMediator mediator) : IRequestHandler<Request, List<MonthlyEarningsDto>>
    {
        private readonly ShopContext _db = db;
        private readonly IMediator _mediator = mediator;

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

                var RevenueDelivery = orders.Sum(o => Order.DeliveryFee) * 0.8m;
                var RevenueTips = 0m;
                foreach(var order in orders)
                {
                    RevenueTips += await _mediator.Send(new GetTipAmount.Request(order.Id));
                }
                var TotalRevenue = RevenueDelivery + RevenueTips;

                result.Add(new MonthlyEarningsDto(month, orders.Count(), RevenueDelivery, RevenueTips, TotalRevenue));
            }

            return result;
        }
    }
}
