using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.webapp.Pages.Shared;

[Authorize(Roles = "Admin,Courier,Customer")]
public class ProfileModel : PageModel
{
    private readonly UserManager<User> _userManager;
    public User CurrentUser;

    public ProfileModel(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGetAsync()
    {   
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        CurrentUser = user;
        return Page();
    }

    


}
