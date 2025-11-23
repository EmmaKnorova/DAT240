using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Dto;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;
using TarlBreuJacoBaraKnor.webapp.SharedKernel;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;

public class CartCheckout
{
    public record Request(Guid CartId, Ordering.Location Location, Guid UserId, string Notes = "", decimal DeliveryFee = 0, string PaymentIntentId = "") : IRequest<Response>;

    public record Response(bool success, Guid OrderId, string[] Errors);

    public class Handler : IRequestHandler<Request, Response>
    {
        private readonly ShopContext _db;
        private readonly IOrderingService _orderingService;

        public Handler(ShopContext db, IOrderingService orderingService)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _orderingService = orderingService ?? throw new ArgumentNullException(nameof(orderingService));
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            // Retrieve cart with items based on cart id
            var cart = await _db.ShoppingCarts.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == request.CartId, cancellationToken);

            if (cart == null)
            {
                return new Response(false, Guid.Empty, new[] { "Cart not found" });
            }

            // Retrieve user
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                return new Response(false, Guid.Empty, new[] { "User not found" });
            }

            if (request.Location == null)
            {
                return new Response(false, Guid.Empty, new[] { "Location cannot be null" });
            }

            // Convert cart items to array of order line DTOs
            var orderLines = cart.Items.Select(item => new OrderLineDto(item.Sku, item.Name, item.Count, item.Price)).ToArray();

            var orderId = await _orderingService.PlaceOrder(request.Location, user, orderLines, request.Notes, request.DeliveryFee, request.PaymentIntentId);

            _db.ShoppingCarts.Remove(cart);

            await _db.SaveChangesAsync(cancellationToken);

            return new Response(true, orderId, Array.Empty<string>());
        }
    }
}