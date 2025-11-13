using MediatR;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;

public class GetByCustomerName
{
    public record Request(string Name) : IRequest<User?>;

    public class Handler : IRequestHandler<Request, User?>
    {
        private readonly ShopContext _db;

        public Handler(ShopContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public async Task<User?> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _db.Users.SingleOrDefaultAsync(us => us.Name == request.Name, cancellationToken);
            return user;
        }
    }
}
