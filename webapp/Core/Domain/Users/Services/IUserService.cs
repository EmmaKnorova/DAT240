using TarlBreuJacoBaraKnor.Core.Domain.Users.DTOs;
using TarlBreuJacoBaraKnor.SharedKernel;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Core.Domain.Users.Services;

public interface IUserService
{
    public Task<Result> RegisterInternalUser(RegisterInputModel registerInputModel);
    public Task<User?> GetUserByEmail(string email);
    public Task<Result<string>> LogInInternalUser(LoginInputModel loginDto);
    public Task ApproveUserState(string userId, CancellationToken cancellationToken);
    public Task DeclineUserState(string userId, CancellationToken cancellationToken);
}