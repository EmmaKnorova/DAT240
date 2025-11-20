using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;
using Microsoft.AspNetCore.Authorization;

namespace TarlBreuJacoBaraKnor.webapp.Pages.Customer.Cart;
[Authorize(Roles = "Customer")]
public class CartCheckoutModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IPaymentService _paymentService;

    public CartCheckoutModel(IMediator mediator, IPaymentService paymentService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
    }

    [BindProperty]
    public string Building { get; set; } = "";
    [BindProperty]
    public string RoomNumber { get; set; } = "";
    [BindProperty]
    public string Notes { get; set; } = "";

    public string[] Errors { get; private set; } = Array.Empty<string>();

    public async Task<IActionResult> OnGetAsync()
    {
        // Get the current user's ID from the authentication cookie
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return RedirectToPage("/Identity/Login");
        }

        // Get the user's cart from the database
        var cartResponse = await _mediator.Send(new GetCartByUserId.Request(userId));
        
        if (!cartResponse.CartId.HasValue)
        {
            return RedirectToPage("/Customer/Cart/Cart");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return RedirectToPage("/Identity/Login");
        }

        // Get the user's cart
        var cartResponse = await _mediator.Send(new GetCartByUserId.Request(userId));
        if (!cartResponse.CartId.HasValue)
        {
            return RedirectToPage("/Customer/Cart/Cart");
        }

        var cart = await _mediator.Send(new Get.Request(cartResponse.CartId.Value));
        if (cart is null || !cart.Items.Any())
        {
            Errors = new[] { "Your cart is empty." };
            return Page();
        }

        // Define delivery fee (fixed or calculated)
        decimal deliveryFee = 50m; // example: 50 NOK

        // Create Stripe checkout session with cart items + delivery fee
        var (checkoutUrl, paymentIntentId) = _paymentService.CreatePaymentSession(cart, deliveryFee, "nok");


        // Store necessary order information in TempData
        TempData["CartId"] = cartResponse.CartId.Value.ToString();
        TempData["UserId"] = userId.ToString();
        TempData["Building"] = Building;
        TempData["RoomNumber"] = RoomNumber;
        TempData["Notes"] = Notes ?? "";
        TempData["DeliveryFee"] = deliveryFee.ToString();
        TempData["PaymentIntentId"] = paymentIntentId;


        // Redirect user to Stripe checkout page
        return Redirect(checkoutUrl);
    }
}