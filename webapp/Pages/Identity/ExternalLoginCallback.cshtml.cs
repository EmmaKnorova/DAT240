using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TarlBreuJacoBaraKnor.Pages.Shared;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.webapp.Pages.Identity;

[AllowAnonymous]
public class ExternalLoginCallbackModel(SignInManager<User> signInManager, UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager) : BasePageModel(userManager, signInManager)
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager = roleManager;

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        public string Email { get; set; }
    }

    public bool ShowConfirmAccountForm { get; set; }
    public string ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Error loading external login information.";
            return RedirectToPage("/Identity/Login");
        }

        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true);

        if (signInResult.Succeeded)
        {
            return LocalRedirect("~/");
        }

        ShowConfirmAccountForm = true;

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        Input = new InputModel { Email = email };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Error loading external login information during confirmation.";
            return RedirectToPage("/Identity/Login");
        }

        var user = new User { UserName = Input.Email, Email = Input.Email };
        var createResult = await _userManager.CreateAsync(user);

        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            ShowConfirmAccountForm = true;
            return Page();
        }

        await _userManager.AddToRoleAsync(user, Roles.Customer.ToString());

        var loginResult = await _userManager.AddLoginAsync(user, info);
        if (!loginResult.Succeeded)
        {
            foreach (var error in loginResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            ShowConfirmAccountForm = true;
            return Page();
        }

        await _signInManager.SignInAsync(user, isPersistent: false);

        return LocalRedirect("~/");
    }
}