using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.Pages.Admin.Helpers;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Pages.Admin;

[ServiceFilter(typeof(RequireChangingPasswordFilter))]
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

}
