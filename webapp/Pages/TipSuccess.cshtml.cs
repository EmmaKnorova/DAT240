using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stripe.Checkout;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.webapp.Pages
{
    public class TipSuccessModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly UserManager<User> _userManager;

        public TipSuccessModel(IMediator mediator, UserManager<User> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string session_id, Guid orderId)
        {
            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(session_id);
            var paymentIntentId = session.PaymentIntentId;

            var user = await _userManager.GetUserAsync(HttpContext.User);
            var orders = await _mediator.Send(new Core.Domain.Ordering.Pipelines.Get.Request(user.Id));
            var order = orders.FirstOrDefault(o => o.Id == orderId);

            if (order == null) return RedirectToPage("/Customer/OrderOverview");

            if (!string.IsNullOrEmpty(order.TipPaymentIntentId))
                return RedirectToPage("/Customer/OrderOverview");

            order.TipPaymentIntentId = paymentIntentId;
            await _mediator.Send(new UpdateOrder.Request(order));

            return Page();
        }
    }
}