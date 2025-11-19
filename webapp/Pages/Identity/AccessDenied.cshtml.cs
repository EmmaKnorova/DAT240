using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace TarlBreuJacoBaraKnor.Pages.Identity;

[Authorize]
public class AccessDeniedModel : PageModel
{
    public string ReturnUrl { get; set; }


    public void OnGet(string returnUrl = null)
    {
        ReturnUrl = returnUrl ?? HttpContext.Request.Path + HttpContext.Request.QueryString;
    }


    public async Task<IActionResult> OnPostSignOut()
    {
        await HttpContext.SignOutAsync();
        return LocalRedirect("/");
    }
}