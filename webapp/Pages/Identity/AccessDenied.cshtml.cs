using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;

namespace TarlBreuJacoBaraKnor.Pages.Identity;

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