using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Pages.Admin.Helpers;

public class RequireChangingPasswordFilter : IAsyncActionFilter
{
    private readonly UserManager<User> _userManager;

    public RequireChangingPasswordFilter(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;
        
        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
        {
            await next();
            return;
        }

        var user = await _userManager.GetUserAsync(httpContext.User);
        if (user is null)
        {
            await next();
            return;
        }

        if (!user.ChangePasswordOnFirstLogin)
        {
            await next();
            return;
        }

        context.Result = new RedirectToPageResult("/Admin/Identity/ChangeDefaultPassword");
    }
}