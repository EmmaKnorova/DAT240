using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;


namespace TarlBreuJacoBaraKnor.webapp.Pages.Courier;

[Authorize(Roles = "Courier")]
public class OrderOverviewModel : PageModel
{

    public List<Order> ActiveOrders { get; set; } = new();
    public List<Order> PastOrders { get; set; } = new();
    private readonly IMediator _mediator;
    private readonly UserManager<User> _userManager;
    private User Courier;

    public OrderOverviewModel(IMediator mediator, UserManager<User> userManager)
    {
        _mediator = mediator;
        _userManager = userManager;
    }
    public async Task OnGetAsync()
    {
        Courier = await _userManager.GetUserAsync(HttpContext.User);
        var orders = await _mediator.Send(new Get.Request(Courier.Id));
        
        ActiveOrders = orders
        .Where(o=> o.Status == Status.Being_picked_up || o.Status == Status.On_the_way)
        .ToList();

        PastOrders = orders
        .Where(o => o.Status == Status.Delivered || o.Status == Status.Cancelled)
        .ToList();
    }
    public async Task<IActionResult> OnPostCancelAsync(Guid orderId)
    {
        Courier = await _userManager.GetUserAsync(HttpContext.User);
        var order = (await _mediator.Send(new Get.Request(Courier.Id)))
            .FirstOrDefault(o => o.Id == orderId);

        if (order == null || (order.Status != Status.Submitted && order.Status != Status.Being_picked_up))
            return RedirectToPage();

        order.Status = Status.Cancelled;
        await _mediator.Send(new UpdateOrder.Request(order));

        return RedirectToPage();
    }

    public IActionResult OnPostDetails(Guid orderId)
    {
        return RedirectToPage("/Courier/OrderDetail", new { id = orderId });
    }

}