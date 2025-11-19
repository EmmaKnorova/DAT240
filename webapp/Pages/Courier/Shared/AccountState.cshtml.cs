using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TarlBreuJacoBaraKnor.Core.Domain.Identity.Entities;
using TarlBreuJacoBaraKnor.Pages.Shared;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Pages.Courier.Shared;

[Authorize(Roles = "Courier")] 
public class AccountStateModel(UserManager<User> userManager, SignInManager<User> signInManager) : BasePageModel(userManager, signInManager)
{
    public required string UserAccountState { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null)
            return LocalRedirect("/");
        
        UserAccountState = user.AccountState.ToString();

        if (UserAccountState == AccountStates.Approved.ToString())
            return LocalRedirect("/Courier/Dashboard");
        return Page();
    }
}