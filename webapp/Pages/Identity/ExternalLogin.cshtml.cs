using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Services;

namespace TarlBreuJacoBaraKnor.Pages.Identity;

public class ExternalLoginModel(IExternalAuthService authService) : PageModel
{
    private readonly IExternalAuthService _authService = authService;

    public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
    {
        if (remoteError != null)
        {
            return RedirectToPage("/Identity/Login", new { ReturnUrl = returnUrl, ErrorMessage = $"Error from external provider: {remoteError}" });
        }

        return await _authService.ProcessGoogleCallbackAsync(returnUrl);
    }
}