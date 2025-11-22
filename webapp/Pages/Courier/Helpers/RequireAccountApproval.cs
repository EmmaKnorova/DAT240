using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Entities;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.webapp.Pages.Courier.Helpers;

public class RequireAccountApprovalFilter(UserManager<User> userManager) : IAsyncPageFilter
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
            AccountStates currentAccountState = user.AccountState;
            if (currentAccountState is AccountStates.Pending or AccountStates.Declined)
            {
                context.Result = new RedirectToPageResult("/Courier/Shared/AccountState");
                return;
            }
        }

        await next();
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;
}