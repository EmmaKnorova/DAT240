using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Pages.Admin.Helpers;

public class RequireChangingPasswordFilter(UserManager<User> userManager) : IAsyncPageFilter
{
    private readonly UserManager<User> _userManager = userManager;

    public async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        var httpContext = context.HttpContext;
        var user = await _userManager.GetUserAsync(httpContext.User);

        if (user != null)
        {
            bool firstLogin = user.ChangePasswordOnFirstLogin;
            string currentUrl = context.HttpContext.Request.Path;

            if (firstLogin && !currentUrl.StartsWith("/Admin/Identity/ChangeDefaultPassword"))
            {
                context.Result = new RedirectToPageResult("/Admin/Identity/ChangeDefaultPassword");
                return;
            }
        }

        await next();
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;
}