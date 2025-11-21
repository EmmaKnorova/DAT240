using MediatR;
using TarlBreuJacoBaraKnor.Core.Domain.Users.DTOs;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Services;
using TarlBreuJacoBaraKnor.SharedKernel;

namespace TarlBreuJacoBaraKnor.Core.Domain.Users.Pipelines;

public class RegisterInternalUser
{
    public record Request(RegisterInputModel RegistrationDto) : IRequest<Result>;

    public class Handler(IUserService userService) : IRequestHandler<Request, Result>
    {
        private IUserService _userService = userService;
        public async Task<Result> Handle(Request request, CancellationToken cancellationToken)
        {
            return await _userService.RegisterInternalUser(request.RegistrationDto);
        }
    }
}