using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;

namespace TarlBreuJacoBaraKnor.webapp.Pages
{
    public class OrderOverviewModel : PageModel
    {

        public List<Order> Orders { get; set; } = new();
        private readonly IMediator _mediator;

        public OrderOverviewModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task OnGetAsync()
        => Orders = await _mediator.Send(new Core.Domain.Ordering.Pipelines.Get.Request());

        public IActionResult OnPost(Guid orderId)
        {
            return RedirectToPage("/OrderDetail", new { id = orderId });
        }
    }
}
