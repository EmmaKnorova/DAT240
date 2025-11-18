using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.Core.Domain.Identity.Entities;

namespace TarlBreuJacoBaraKnor.Core.Domain.Users.Services;

public class Decline
{
    public record Request(string UserId) : IRequest;

    public class Handler(ShopContext db, UserManager<User> userManager) : IRequestHandler<Request>
    {
        private readonly ShopContext _db = db ?? throw new ArgumentNullException(nameof(db));
        private readonly UserManager<User> _userManager = userManager;

        public async Task Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user != null)
            {
                user.AccountState = AccountStates.Declined;
                await _db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}