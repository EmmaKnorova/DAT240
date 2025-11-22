using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using Stripe.Checkout;


namespace TarlBreuJacoBaraKnor.webapp.Pages
{

    public class PaymentSuccessModel : PageModel
    {
        private readonly IMediator _mediator;

        public PaymentSuccessModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> OnGetAsync(string session_id)
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

            var deliveryFee = decimal.Parse(TempData["DeliveryFee"]?.ToString() ?? "0");
            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(session_id);
            var paymentIntentId = session.PaymentIntentId;

            var result = await _mediator.Send(new CartCheckout.Request(
                cartId,
                location,
                userId,
                location.Notes,
                deliveryFee,
                paymentIntentId 
            ));

            if (result.success)
            {
                return Page();
            }

            return RedirectToPage("/Customer/Cart/Cart");
        }
    }
}