using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stripe;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Events;
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

        var previousStatus = _order.Status;

        _order.Status = _order.Status switch
        {
            Status.Submitted => Status.Being_picked_up,
            Status.Being_picked_up => Status.On_the_way,
            Status.On_the_way => Status.Delivered,
            _ => _order.Status
        };

        await _mediator.Send(new UpdateOrder.Request(_order));

        if (previousStatus == Status.Submitted && _order.Status == Status.Being_picked_up)
        {
            await _mediator.Publish(new OrderAccepted(OrderId));
        }
        else if (previousStatus == Status.Being_picked_up && _order.Status == Status.On_the_way)
        {
            await _mediator.Publish(new OrderSent(OrderId));
        }
        else if (previousStatus == Status.On_the_way && _order.Status == Status.Delivered)
        {
            await _mediator.Publish(new OrderDelivered(OrderId));
        }

        return RedirectToPage("/Courier/OrderOverview");
    }
}
