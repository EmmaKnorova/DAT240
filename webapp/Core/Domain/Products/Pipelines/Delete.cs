using System;
using System.Threading;
using System.Threading.Tasks;
using TarlBreuJacoBaraKnor.webapp.Core.Exceptions;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Products.Pipelines;

public class Delete
{
    public record Request(int Id) : IRequest;

    public class Handler : IRequestHandler<Request>
    {
        private readonly ShopContext _db;

        public Handler(ShopContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public async Task Handle(Request request, CancellationToken cancellationToken)
        {
            var item = await _db.FoodItems.SingleOrDefaultAsync(fi => fi.Id == request.Id, cancellationToken);
            if (item is null) throw new EntityNotFoundException($"FoodItem with Id {request.Id} was not found in the database");
            _db.Remove(item);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}