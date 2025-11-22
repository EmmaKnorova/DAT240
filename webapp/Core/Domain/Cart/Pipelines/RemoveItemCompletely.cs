using MediatR;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;

public static class RemoveItemCompletely
{
    public record Request(Guid CartId, int ItemSku) : IRequest<Result>;

    public record Result(bool Success, string? Message = null);

    public class Handler : IRequestHandler<Request, Result>
    {
        private readonly ShopContext _db;

        public Handler(ShopContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<Result> Handle(Request request, CancellationToken cancellationToken)
        {
            var cart = await _db.ShoppingCarts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == request.CartId, cancellationToken);

            if (cart == null)
            {
                return new Result(false, "Cart not found");
            }

            try
            {
                cart.RemoveItemCompletely(request.ItemSku);
                await _db.SaveChangesAsync(cancellationToken);
                return new Result(true);
            }
            catch (InvalidOperationException ex)
            {
                return new Result(false, ex.Message);
            }
        }
    }
}