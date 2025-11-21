using MediatR;

namespace TarlBreuJacoBaraKnor.Core.Domain.Users.Pipelines;

public class RegisterInternalUser
{
    public record Request(string UserId) : IRequest;

    public class Handler : IRequestHandler<Request>
    {
        public async Task Handle(Request request, CancellationToken cancellationToken)
        {
            
        }
    }
}