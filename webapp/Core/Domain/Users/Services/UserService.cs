using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Entities;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.Core.Domain.Users.Services;

public class UserService(UserManager<User> userManager, SignInManager<User> signInManager, ShopContext db) : IUserService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly ShopContext _db = db;

    public async Task ApproveUserState(string userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.AccountState = AccountStates.Approved;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeclineUserState(string userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.AccountState = AccountStates.Declined;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public Task<bool> CreateExternalUserAsync(string email, List<Claim> claims, string loginProvider, string providerKey)
    {
        throw new NotImplementedException();
    }

    public Task<List<User>> GetUsersByRole(string role)
    {
        throw new NotImplementedException();
    }

    public Task<bool> LoginWithExternalProvider(HttpContext httpContext)
    {
        throw new NotImplementedException();
    }

    public Task<IActionResult> RegisterExternalUser()
    {
        throw new NotImplementedException();
    }

    public Task<IActionResult> RegisterUser()
    {
        throw new NotImplementedException();
    }
}