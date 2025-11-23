using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using webapp.Core.Domain.Ordering.Pipelines;


namespace TarlBreuJacoBaraKnor.webapp.Pages.Courier;

[Authorize(Roles = "Courier")]
public class OrderOverviewModel : PageModel
{

    public List<Order> ActiveOrders { get; set; } = new();
    public List<Order> PastOrders { get; set; } = new();
    private readonly IMediator _mediator;
    private readonly UserManager<User> _userManager;
    public Dictionary<Guid, decimal> Tips { get; set; } = new();
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
        .Where(o => o.Status == Status.Being_picked_up || o.Status == Status.On_the_way)
        .OrderByDescending(o => o.OrderDate)
        .ToList();

        PastOrders = orders
        .Where(o => o.Status == Status.Delivered || o.Status == Status.Cancelled)
        .OrderByDescending(o => o.OrderDate)
        .ToList();

        foreach (var o in PastOrders)
        {
        
            var tip = await _mediator.Send(new GetTipAmount.Request(o.Id));
            Tips[o.Id] = tip;
 
        } 
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