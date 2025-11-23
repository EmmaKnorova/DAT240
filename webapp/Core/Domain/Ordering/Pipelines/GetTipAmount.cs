using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stripe;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace webapp.Core.Domain.Ordering.Pipelines;

public class GetTipAmount
{
    public record Request(Guid orderId) : IRequest<decimal>;

    public class Handler : IRequestHandler<Request, decimal>
    {   
        private readonly ShopContext _db;
        public decimal Tip;
        public Handler(ShopContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<decimal> Handle(Request request, CancellationToken cancellationToken)
        {
            Order? _order = await _db.Orders
                .SingleOrDefaultAsync(or => or.Id == request.orderId, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_order.TipPaymentIntentId))
            {
                var service = new PaymentIntentService();
                var pi = await service.GetAsync(_order.TipPaymentIntentId);

                Tip = pi.Amount / 100m;
            }
            else
            {
                Tip = 0; 
            }
            return Tip;
        }
    }
}