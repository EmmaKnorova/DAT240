using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stripe;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using webapp.Core.Domain.Ordering.Pipelines;

namespace TarlBreuJacoBaraKnor.webapp.Pages.Courier;

[Authorize(Roles = "Courier")]
public class OrderDetailModel : PageModel
{
    public Order _order { get; set; }
    private readonly IMediator _mediator;
    private readonly UserManager<User> _userManager;
    public User Courier;
    public decimal Tip {get; private set;}

    [BindProperty]
    public Guid OrderId { get; set; }

    public OrderDetailModel(IMediator mediator, UserManager<User> userManager)
    {
        _mediator = mediator;
        _userManager = userManager;
    }

    public async Task OnGet(Guid id)
    {
        OrderId = id;
        Console.WriteLine(OrderId);
        _order = await _mediator.Send(new GetSpecificOrder.Request(id));
        Tip = await _mediator.Send(new GetTipAmount.Request(id));
    }

        public async Task<IActionResult> OnPostAsync()
    {
        _order = await _mediator.Send(new GetSpecificOrder.Request(OrderId));
        Courier = await _userManager.GetUserAsync(HttpContext.User);

        if (_order == null)
            return BadRequest("Order not found");

        if (_order.Courier == null)
            _order.Courier = Courier;

        _order.Status = _order.Status switch
        {
            Status.Submitted => Status.Being_picked_up,
            Status.Being_picked_up => Status.On_the_way,
            Status.On_the_way => Status.Delivered,
            _ => _order.Status
        };

        await _mediator.Send(new UpdateOrder.Request(_order));

        return RedirectToPage("/Courier/OrderOverview");
    }
}
