using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TarlBreuJacoBaraKnor.Core.Domain.Users.DTOs;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Entities;
using TarlBreuJacoBaraKnor.SharedKernel;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.Core.Domain.Users.Services;

public class UserService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ShopContext db, 
    ILogger<UserService> logger, 
    RoleManager<IdentityRole<Guid>> roleManager) : IUserService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly ShopContext _db = db;
    private readonly ILogger<UserService> _logger = logger;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager = roleManager;
    private readonly SignInManager<User> _signInManager = signInManager;

        public List<string> PermittedLoginRoles { get; set; } = [Roles.Customer.ToString(), Roles.Courier.ToString()];

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

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<Result<string>> LogInInternalUser(LoginInputModel loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        
        if (user == null)
            return Result<string>.Failure("Invalid email or password.");

        var userRoles = await _userManager.GetRolesAsync(user);
        if (!userRoles.Any(PermittedLoginRoles.Contains))
            return Result<string>.Failure("User is not a Courier or Customer.");
        
        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            loginDto.Password,
            lockoutOnFailure: false
        );

        if (result.Succeeded)
        {
            _logger.LogInformation($"User logged in: {user.Email}");
            await _signInManager.SignInAsync(user, isPersistent: false);

            string defaultRedirectPath;
            if (userRoles.Contains(Roles.Customer.ToString()))
                defaultRedirectPath = "/Customer/Menu";
            else if (userRoles.Contains(Roles.Courier.ToString()))
                defaultRedirectPath = "/Courier/Dashboard";
            else
                defaultRedirectPath = "/";

            return Result<string>.Success(defaultRedirectPath);
        }

        if (result.IsLockedOut)
            return Result<string>.Failure("Account is locked. Try again later.");

        return Result<string>.Failure("Invalid email or password.");
    }

    public async Task<Result> RegisterInternalUser(RegisterInputModel registerInputModel)
    {
        var userFoundByEmail = await GetUserByEmail(registerInputModel.Email);
        Console.WriteLine($"This is the user with the email: {userFoundByEmail}");
        if (userFoundByEmail != null)
        {
            return Result.Failure($"A user has already registered with this email address: {registerInputModel.Email}");
        }

        var user = new User
        {
            UserName = registerInputModel.UserName,
            Email = registerInputModel.Email,
            Name = registerInputModel.Name,
            PhoneNumber = registerInputModel.PhoneNumber,
            Address = registerInputModel.Address,
            City = registerInputModel.City,
            PostalCode = registerInputModel.PostalCode,
            EmailConfirmed = true,
        };

        var createResult = await _userManager.CreateAsync(user, registerInputModel.Password);
        if (!createResult.Succeeded)
        {
            return Result.Failure(createResult);
        }

        _logger.LogInformation($"New user has registered: {registerInputModel.Name}.");

        var userSelectedRole = registerInputModel.Role;
        if (!await _roleManager.RoleExistsAsync(userSelectedRole))
        {
            _logger.LogError($"Registration failed: Selected role {userSelectedRole} does not exist.");
            return Result.Failure("Selected user role is invalid.");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, userSelectedRole);
        if (!roleResult.Succeeded)
        {
            return Result.Failure(roleResult);
        }

        return Result.Success();
    }
}