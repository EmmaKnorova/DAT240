using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.Core.Domain.Users.DTOs;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Pages.Admin.Identity;

[Authorize]
public class AdminChangeDefaultPasswordModel(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILogger<AdminChangeDefaultPasswordModel> logger) : PageModel
{
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly UserManager<User> _userManager = userManager;
    private readonly ILogger<AdminChangeDefaultPasswordModel> _logger = logger;

    [BindProperty] public required ChangeDefaultPasswordModel Input { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null)
        {
            return RedirectToPage("/Admin/Identity/AdminLogin");
        }

        if (!user.ChangePasswordOnFirstLogin)
        {
            return Redirect("/Admin/Dashboard");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null)
        {
            return RedirectToPage("/Admin/Identity/AdminLogin");
        }

        if (!user.ChangePasswordOnFirstLogin)
            return Redirect("/Admin/Dashboard");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, Input.Password);

        if (result.Succeeded)
        {
            user.ChangePasswordOnFirstLogin = false;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var err in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);

                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("Admin changed default password successfully.");

            return Redirect("/Admin/Dashboard");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return Page();
    }
}