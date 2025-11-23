using MediatR;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;
using webapp.Core.Domain.Ordering.Pipelines;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;

public class GetMonthlyCourierStats
{
    public record Request(int Year, int Month,Guid CourierId) : IRequest<Response>;

    public record StatusCountDto(string Status, int Count);

    public record Response(
        int TotalOrders,
        decimal TotalRevenue,
        decimal TipRevenue
    );

    public class Handler : IRequestHandler<Request, Response>
    {
        private readonly ShopContext _db;
        private IMediator _mediator;

        public Handler(ShopContext db, IMediator mediator)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }


        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var startOfMonth = new DateTimeOffset(request.Year, request.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var endOfMonth = startOfMonth.AddMonths(1);


            var monthlyOrders = await _db.Orders
                .Where(o => o.OrderDate >= startOfMonth && o.OrderDate < endOfMonth)
                .Where(o => o.Courier.Id == request.CourierId)
                .ToListAsync(cancellationToken);

            var allOrders = await _db.Orders
            .Where(o => o.Courier.Id == request.CourierId)
            .ToListAsync(cancellationToken);

            var totalOrders = allOrders
                .Where(o => o.Status == Status.Delivered)
                .Count();

            var totalRevenue = allOrders
                .Where(o => o.Status == Status.Delivered)
                .Sum(o =>
                Order.DeliveryFee)*0.8m;

            var revenueTips = 0m;
                foreach(var order in allOrders)
                {
                    revenueTips += await _mediator.Send(new GetTipAmount.Request(order.Id));
                }
      



            return new Response(totalOrders, totalRevenue, revenueTips);
        }
    }
}