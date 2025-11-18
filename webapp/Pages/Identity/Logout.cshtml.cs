using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Pages.Identity;

public class LogoutModel(SignInManager<User> signInManager) : PageModel
{
    private readonly SignInManager<User> _signInManager = signInManager;

    public async Task<IActionResult> OnGetAsync()
    {
        return NotFound();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _signInManager.SignOutAsync();
        return RedirectToPage("/Index");
    }
}
