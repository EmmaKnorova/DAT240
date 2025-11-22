using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace TarlBreuJacoBaraKnor.Core.Domain.Users.Services;

public interface IExternalAuthService
{
    public AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl);
    public Task<IActionResult> ProcessGoogleCallbackAsync(string returnUrl = null);
}
