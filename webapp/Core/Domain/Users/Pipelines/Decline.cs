using MediatR;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Services;

namespace TarlBreuJacoBaraKnor.Core.Domain.Users.Pipelines;

public class Decline
{
    public record Request(string UserId) : IRequest;

    public class Handler(IUserService userService) : IRequestHandler<Request>
    {
        private IUserService _userService = userService;
        public async Task Handle(Request request, CancellationToken cancellationToken)
        {
            await _userService.DeclineUserState(request.UserId, cancellationToken);
        }
    }
}