using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;

public class AddItem
{
    public record Request(
        int ItemId,
        string ItemName,
        decimal ItemPrice,
        Guid UserId,
        Guid CartId) : IRequest<Unit>;

    public record Response(bool Success, string[] Errors);

    public class Handler : IRequestHandler<Request, Unit>
    {
        private readonly ShopContext _db;

        public Handler(ShopContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            var cart = await _db.Set<ShoppingCart>()
                .Include(c => c.Items)
                .SingleOrDefaultAsync(c => c.Id == request.CartId, cancellationToken);
            if (cart == null)
            {
                cart = new ShoppingCart(request.CartId, request.UserId);
                _db.Set<ShoppingCart>().Add(cart);
            }
            
            cart.AddItem(request.ItemId, request.ItemName, request.ItemPrice);

            await _db.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}