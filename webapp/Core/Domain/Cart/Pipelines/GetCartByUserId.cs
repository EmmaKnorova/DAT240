using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;

public class GetCartByUserId
{
    public record Request(Guid UserId) : IRequest<Response>;

    public record Response(Guid? CartId);

    public class Handler : IRequestHandler<Request, Response>
    {
        private readonly ShopContext _db;

        public Handler(ShopContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var cart = await _db.ShoppingCarts
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

            return new Response(cart?.Id);
        }
    }
}