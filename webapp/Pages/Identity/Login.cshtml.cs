using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TarlBreuJacoBaraKnor.Core.Domain.Users.DTOs;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Services;
using TarlBreuJacoBaraKnor.Pages.Shared;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Pages.Identity;

[AllowAnonymous]
public class LoginModel(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILogger<LoginModel> logger,
    IExternalAuthService authService) : BasePageModel(userManager, signInManager)
{
    private readonly ILogger<LoginModel> _logger = logger;
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }
    [BindProperty] public required LoginInputModel Input { get; set; }
    public List<string> PermittedRoles { get; set; } = [Roles.Customer.ToString(), Roles.Courier.ToString()];
    private readonly IExternalAuthService _authService = authService;
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

    public IActionResult OnPostExternalLogin(string provider, string returnUrl = null)
    {
        var redirectUrl = Url.Page("/Identity/ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
        var properties = _authService.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
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