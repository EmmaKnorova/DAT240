using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Pages.Admin;

[Authorize(Roles = "Admin")]
public class AdminDashboardModel(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILogger<AdminDashboardModel> logger,
    RoleManager<IdentityRole<Guid>> roleManager) : PageModel
{
    public Order _order;
    private readonly IMediator _mediator;

    [BindProperty]
    public string? ShipperName { get; set; }

    [BindProperty]
    public Guid OrderId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user.ChangePasswordOnFirstLogin)
            return LocalRedirect("/Admin/Identity/ChangeDefaultPassword");
        return Page();
    }
}
