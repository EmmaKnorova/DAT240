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

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;

public class CartCheckout
{
    public record Request(Guid CartId, Ordering.Location Location, Guid UserId, string Notes = "") : IRequest<Response>;

    public record Response(bool success, Guid OrderId, string[] Errors);

    public class Handler : IRequestHandler<Request, Response>
    {
        private readonly ShopContext _db;
        private readonly IOrderingService _orderingService;
        private readonly IEnumerable<IValidator<ShoppingCart>> _cartValidators;
        private readonly IEnumerable<IValidator<Ordering.Location>> _locationValidators;

        public Handler(ShopContext db, IOrderingService orderingService, IEnumerable<IValidator<ShoppingCart>> cartValidators, IEnumerable<IValidator<Ordering.Location>> locationValidators)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _orderingService = orderingService ?? throw new ArgumentNullException(nameof(orderingService));
            _cartValidators = cartValidators ?? throw new ArgumentNullException(nameof(cartValidators));
            _locationValidators = locationValidators ?? throw new ArgumentNullException(nameof(locationValidators));
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

            var errors = _cartValidators
                    .Select(v => v.IsValid(cart))
                    .Where(result => !result.IsValid)
                    .Select(result => result.Error)
                    .ToArray();

            if (request.Location == null)
            {
                return new Response(false, Guid.Empty, new[] { "Location cannot be null" });
            }

            errors = errors.Concat(_locationValidators
                    .Select(v => v.IsValid(request.Location))
                    .Where(result => !result.IsValid)
                    .Select(result => result.Error)
                    .ToArray()).ToArray();
            
            if (errors.Any())
            {
                return new Response(false, Guid.Empty, errors);
            }

            // Convert cart items to array of order line DTOs
            var orderLines = cart.Items.Select(item => new OrderLineDto(item.Sku, item.Name, item.Count, item.Price)).ToArray();

            var orderId = await _orderingService.PlaceOrder(request.Location, user, orderLines, request.Notes);

            _db.ShoppingCarts.Remove(cart);

            await _db.SaveChangesAsync(cancellationToken);

            return new Response(true, orderId, Array.Empty<string>());
        }
    }
}