using MediatR;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;

public class SaveChanges
{
    public record Request() : IRequest<Unit>;

    public class Handler : IRequestHandler<Request, Unit>
    {
        private readonly ShopContext _db;

        public Handler(ShopContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }
        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            await _db.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
