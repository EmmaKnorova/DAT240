using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.Core.Domain.Users.DTOs;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Pages.Identity;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<RegisterModel> _logger;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    [BindProperty]
    public required RegisterInputModel Input { get; set; }
    public List<string> AvailableRoles { get; set; } = [Roles.Customer.ToString(), Roles.Courier.ToString()];

    public RegisterModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<RegisterModel> logger,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Identity.IsAuthenticated)
                return Redirect("/");
            else return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = new User
        {
            UserName = Input.UserName,
            Email = Input.Email,
            Name = Input.Name,
            PhoneNumber = Input.PhoneNumber,
            Address = Input.Address,
            City = Input.City,
            PostalCode = Input.PostalCode,
            EmailConfirmed = true,
        };

        var userFoundByEmail = await _userManager.FindByEmailAsync(user.Email);
        if (userFoundByEmail != null)
        {
            ModelState.AddModelError(string.Empty, $"A user has already registered with this email address: {user.Email}");
            return Page();
        }

        var result = await _userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        _logger.LogInformation($"New user has registered: {Input.Name}.");

        var userSelectedRole = Input.Role;
        if (!await _roleManager.RoleExistsAsync(userSelectedRole))
        {
            ModelState.AddModelError(string.Empty, "Selected role doesn't exist.");
            return Page();
        }

        var roleResult = await _userManager.AddToRoleAsync(user, userSelectedRole);
        if (!roleResult.Succeeded)
        {
            foreach (var error in roleResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        if (userSelectedRole == "Customer")
            return LocalRedirect("/Customer/Menu");
        return LocalRedirect("/Courier/Dashboard");
    }
}
