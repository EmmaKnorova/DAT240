using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;

namespace TarlBreuJacoBaraKnor.webapp.Pages
{

    public class PaymentSuccessModel : PageModel
    {
        private readonly IMediator _mediator;

        public PaymentSuccessModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!Guid.TryParse(TempData["CartId"]?.ToString(), out var cartId) ||
                !Guid.TryParse(TempData["UserId"]?.ToString(), out var userId))
            {
                return RedirectToPage("/Customer/Cart/Cart");
            }

            var location = new Location
            {
                Building = TempData["Building"]?.ToString() ?? "",
                RoomNumber = TempData["RoomNumber"]?.ToString() ?? "",
                Notes = TempData["Notes"]?.ToString() ?? ""
            };

            var result = await _mediator.Send(new CartCheckout.Request(
                cartId,
                location,
                userId,
                location.Notes
            ));

            if (result.success)
            {
                return Page();
            }

            return RedirectToPage("/Customer/Cart/Cart");
        }
    }
}