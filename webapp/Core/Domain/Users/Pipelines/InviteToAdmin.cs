using MediatR;
using Microsoft.AspNetCore.Identity;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Users.Pipelines;

public class InviteToAdmin
{
    public record Request(Guid UserId) : IRequest<Response>;

    public record Response(bool Success, string[] Errors);

    public class Handler : IRequestHandler<Request, Response>
    {
        private readonly UserManager<User> _userManager;

        public Handler(UserManager<User> userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user == null)
            {
                return new Response(false, new[] { "User not found" });
            }

            if (await _userManager.IsInRoleAsync(user, Roles.Admin.ToString()))
            {
                return new Response(false, new[] { "User is already an administrator" });
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!removeResult.Succeeded)
            {
                return new Response(false, removeResult.Errors.Select(e => e.Description).ToArray());
            }

            var addResult = await _userManager.AddToRoleAsync(user, Roles.Admin.ToString());

            if (!addResult.Succeeded)
            {
                return new Response(false, addResult.Errors.Select(e => e.Description).ToArray());
            }

            return new Response(true, Array.Empty<string>());
        }
    }
}