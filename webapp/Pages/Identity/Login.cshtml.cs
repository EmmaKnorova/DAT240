using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.Core.Domain.Identity.DTOs;
using TarlBreuJacoBaraKnor.Pages.Shared;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Pages.Identity;

[AllowAnonymous]
public class LoginModel(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILogger<LoginModel> logger,
    RoleManager<IdentityRole<Guid>> roleManager) : BasePageModel(userManager, signInManager)
{
    private readonly ILogger<LoginModel> _logger = logger;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager = roleManager;
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }
    [BindProperty] public required LoginInputModel Input { get; set; }
    public List<string> PermittedRoles { get; set; } = [Roles.Customer.ToString(), Roles.Courier.ToString()];

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
        if (!User.Identity.IsAuthenticated)
            return Page();

        if (User.IsInRole(Roles.Customer.ToString()))
            return Redirect("/Customer/Menu");
        else if (User.IsInRole(Roles.Courier.ToString()))
            return Redirect("/Customer/OrderOverview");
        return Redirect("/");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return Page();
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        if (!userRoles.Any(PermittedRoles.Contains)) {
            ModelState.AddModelError(string.Empty, "User is not a Courier or Customer.");
            return Page();
        }
        
        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            Input.Password,
            lockoutOnFailure: false
        );
        
        if (result.Succeeded)
        {
            _logger.LogInformation($"User logged in: {user.Email}");
            await _signInManager.SignInAsync(user, isPersistent: false);

            if (userRoles.Contains(Roles.Customer.ToString()))
                return LocalRedirect("/Customer/Menu");
            else if (userRoles.Contains(Roles.Courier.ToString()))
                return LocalRedirect("/Customer/OrderOverview");
            else if (Url.IsLocalUrl(ReturnUrl))
                return LocalRedirect(ReturnUrl);
            return LocalRedirect("/");
        }
        
        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account is locked. Try again later.");
            return Page();
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return Page();
    }
}