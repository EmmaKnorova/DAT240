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

    public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
    {
        returnUrl ??= Url.Content("~/");

        if (remoteError != null)
        {
            ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
            return RedirectToPage("/Identity/Login");
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Error loading external login information during confirmation.";
            return RedirectToPage("/Identity/Login");
        }

        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true);

        if (!result.Succeeded)
        {
            return LocalRedirect(returnUrl);
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);

        if (email != null)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createUserResult = await _userManager.CreateAsync(user);
                if (!createUserResult.Succeeded)
                {
                    foreach (var error in createUserResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    return RedirectToPage("/Identity/Login");
                }

                await _userManager.AddToRoleAsync(user, "Customer");
            }

            await _userManager.AddLoginAsync(user, info);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Email not received from external provider.");
        return RedirectToPage("/Identity/Login");
    }
}