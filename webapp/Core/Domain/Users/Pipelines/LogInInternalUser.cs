using MediatR;
using TarlBreuJacoBaraKnor.Core.Domain.Users.DTOs;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Services;
using TarlBreuJacoBaraKnor.SharedKernel;

namespace TarlBreuJacoBaraKnor.Core.Domain.Users.Pipelines;

public class LogInInternalUser
{
    public record Request(LoginInputModel LoginDto) : IRequest<Result<string>>;

    public class Handler(IUserService userService) : IRequestHandler<Request, Result<string>>
    {
        private IUserService _userService = userService;
        public async Task<Result<string>> Handle(Request request, CancellationToken cancellationToken)
        {
            return await _userService.LogInInternalUser(request.LoginDto);
        }
    }
}