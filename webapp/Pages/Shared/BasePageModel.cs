using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Pages.Shared;

public abstract class BasePageModel(UserManager<User> userManager,
                     SignInManager<User> signInManager) : PageModel
{
    protected readonly UserManager<User> _userManager = userManager;
    protected readonly SignInManager<User> _signInManager = signInManager;
}