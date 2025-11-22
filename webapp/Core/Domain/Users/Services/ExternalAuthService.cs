using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Core.Domain.Users.Services;

public class ExternalAuthService(
    SignInManager<User> signInManager,
    UserManager<User> userManager) : IExternalAuthService
{
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly UserManager<User> _userManager = userManager;

    public AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl)
    {
        return _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
    }

    public async Task<IActionResult> ProcessGoogleCallbackAsync(string returnUrl = null)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return new BadRequestObjectResult("Error loading external login information.");
        }

        var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        if (signInResult.Succeeded)
        {
            return new LocalRedirectResult(returnUrl ?? "/Customer/Menu");
        }

        if (signInResult.IsNotAllowed)
        {
            return new BadRequestObjectResult("Your account exists but is not verified. Please check your email.");
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var name = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? 
           info.Principal.FindFirstValue(ClaimTypes.Name) ?? 
           "External User";
        var city = info.Principal.FindFirstValue(ClaimTypes.StateOrProvince) ?? string.Empty;
        var address = info.Principal.FindFirstValue(ClaimTypes.StreetAddress) ?? string.Empty;
        var postalCode = info.Principal.FindFirstValue(ClaimTypes.PostalCode) ?? string.Empty;
        
        if (string.IsNullOrEmpty(email))
        {
            return new BadRequestObjectResult("Email not found from Google provider.");
        }

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new User
            {
                UserName = email,
                Name = name,
                Email = email,
                EmailConfirmed = true,
                City = city,
                Address = address,
                PostalCode = postalCode
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return new BadRequestObjectResult(createResult.Errors);
            }

            await _userManager.AddToRoleAsync(user, "Customer");
        }

        var currentLogins = await _userManager.GetLoginsAsync(user);
        if (!currentLogins.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey))
        {
            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                return new BadRequestObjectResult(addLoginResult.Errors);
            }
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        
        return new LocalRedirectResult(returnUrl ?? "/Customer/Menu");
    }
}