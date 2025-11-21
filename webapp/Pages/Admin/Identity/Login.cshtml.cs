using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.Core.Domain.Users.DTOs;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Pages.Admin.Identity;

[AllowAnonymous]
public class AdminLoginModel(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILogger<AdminLoginModel> logger,
    RoleManager<IdentityRole<Guid>> roleManager) : PageModel
{
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly UserManager<User> _userManager = userManager;
    private readonly ILogger<AdminLoginModel> _logger = logger;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager = roleManager;
    private readonly string _defaultUrlRedirectPath = "/Admin/Dashboard"; 
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }
    [BindProperty] public required LoginInputModel Input { get; set; }
    public List<string> PermittedRoles { get; set; } = [Roles.Admin.ToString()];

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content(_defaultUrlRedirectPath);
        if (User.Identity.IsAuthenticated && User.IsInRole(Roles.Admin.ToString()))
                return Redirect(_defaultUrlRedirectPath);
            else return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
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
            if (Url.IsLocalUrl(ReturnUrl))
                return Redirect(ReturnUrl);
            return Redirect(_defaultUrlRedirectPath);
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