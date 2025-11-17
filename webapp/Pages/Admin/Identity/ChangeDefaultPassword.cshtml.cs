using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.Core.Domain.Identity.DTOs;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Pages.Admin.Identity;

[Authorize(Roles = "Admin")]
public class AdminChangeDefaultPasswordModel(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILogger<AdminLoginModel> logger,
    RoleManager<IdentityRole<Guid>> roleManager) : PageModel
{
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly UserManager<User> _userManager = userManager;
    private readonly ILogger<AdminLoginModel> _logger = logger;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager = roleManager;
    [BindProperty] public required ChangeDefaultPasswordModel Input { get; set; }
    public List<string> AllowedRoles { get; set; } = [Roles.Admin.ToString()];

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "User not logged in.");
            return Page();
        }

        if (!user.ChangePasswordOnFirstLogin)
            return LocalRedirect("/Admin/Dashboard");

        if (Input.Password != Input.PasswordConfirmation)
        {
            ModelState.AddModelError(string.Empty, "The password and password confirmation don't match.");
            return Page();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ChangePasswordAsync(user, token, Input.Password);

        if (result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Account is locked. Try again later.");
            return LocalRedirect("/Admin/Dashboard");
        }

        ModelState.AddModelError(string.Empty, "Password change was unsuccessful.");
        return Page();
    }
}
