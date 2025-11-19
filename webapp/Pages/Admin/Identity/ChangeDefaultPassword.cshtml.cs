using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.Core.Domain.Identity.DTOs;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.Pages.Admin.Identity;

[Authorize(Roles = "Admin")]
public class AdminChangeDefaultPasswordModel(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILogger<AdminLoginModel> logger,
    RoleManager<IdentityRole<Guid>> roleManager,
    ShopContext context) : PageModel
{
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly UserManager<User> _userManager = userManager;
    private readonly ILogger<AdminLoginModel> _logger = logger;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager = roleManager;
    private readonly ShopContext _context = context;

    [BindProperty] public required ChangeDefaultPasswordModel Input { get; set; }

    public List<string> PermittedRoles { get; set; } = [Roles.Admin.ToString()];

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

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, Input.Password);

        if (result.Succeeded)
        {
            user.ChangePasswordOnFirstLogin = false;

            _context.Users.Attach(user);
            _context.Entry(user).Property(u => u.ChangePasswordOnFirstLogin).IsModified = true;
            await _context.SaveChangesAsync();

            await _signInManager.RefreshSignInAsync(user);

            _logger.LogInformation("Admin password changed successfully.");

            return LocalRedirect("/Admin/Dashboard");
        }

        ModelState.AddModelError(string.Empty, "Password change was unsuccessful.");
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return Page();
    }
}