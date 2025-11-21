using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TarlBreuJacoBaraKnor.Core.Domain.Users.DTOs;
using TarlBreuJacoBaraKnor.SharedKernel;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Core.Domain.Users.Services;

public interface IUserService
{
    public Task<Result> RegisterInternalUser(RegisterInputModel registerInputModel);
    public Task<IActionResult> RegisterExternalUser();
    public Task<List<User>> GetUsersByRole(string role);
    public Task<bool> LoginWithExternalProvider(HttpContext httpContext);
    public Task<bool> CreateExternalUserAsync(string email, List<Claim> claims, string loginProvider, string providerKey);
    public Task ApproveUserState(string userId, CancellationToken cancellationToken);
    public Task DeclineUserState(string userId, CancellationToken cancellationToken);
}