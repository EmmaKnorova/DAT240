using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TarlBreuJacoBaraKnor.Pages.Shared;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Pages.Courier.Helpers;

namespace TarlBreuJacoBaraKnor.Pages.Courier;

[ServiceFilter(typeof(RequireAccountApprovalFilter))]
[Authorize(Roles = "Courier")]
public class CourierDashboardModel(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILogger<CourierDashboardModel> logger,
    RoleManager<IdentityRole<Guid>> roleManager) : BasePageModel(userManager, signInManager)
{
    public Order _order;
    private readonly IMediator _mediator;

    [BindProperty]
    public string? ShipperName { get; set; }

    [BindProperty]
    public Guid OrderId { get; set; }

}