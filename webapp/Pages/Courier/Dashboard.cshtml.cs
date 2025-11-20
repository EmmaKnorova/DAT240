using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TarlBreuJacoBaraKnor.Pages.Shared;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Pages.Courier.Helpers;

namespace TarlBreuJacoBaraKnor.Pages.Courier;


[Authorize(Roles = "Courier")]
public class CourierDashboardModel : BasePageModel
{
    private readonly IMediator _mediator;

    public List<Order> ActiveOrders { get; set; } = new();
    public Order _order;

    public CourierDashboardModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<CourierDashboardModel> logger,
        RoleManager<IdentityRole<Guid>> roleManager,
        IMediator mediator)
        : base(userManager, signInManager)
    {
        _mediator = mediator;
    }

    public async Task OnGetAsync()
    {
        var orders = await _mediator.Send(new GetAllOrders.Request());

        ActiveOrders = orders
            .Where(o => o.Status == Status.Submitted)
            .ToList();
    }

    public async Task<ActionResult> OnPostAsync(Guid orderId)
    {
        return RedirectToPage("/Courier/OrderDetail", new { id = orderId });
    }


}
