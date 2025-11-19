using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;

namespace TarlBreuJacoBaraKnor.webapp.Pages.Customer;

public class OrderDetailModel : PageModel
{
    public Order _order;
    private readonly IMediator _mediator;

    [BindProperty]
    public string? ShipperName { get; set; }

    [BindProperty]
    public Guid OrderId { get; set; }

    public OrderDetailModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task OnGet(Guid id)
    {
        OrderId = id;
        _order = await _mediator.Send(new GetSpecificOrder.Request(id));
    }

    public async Task<ActionResult> OnPostAsync()
    {
        _order = await _mediator.Send(new GetSpecificOrder.Request(OrderId));
        if (_order == null)
        {
            return BadRequest("Order not found.");
        }


        return RedirectToPage(new { id = OrderId });
    }
}
