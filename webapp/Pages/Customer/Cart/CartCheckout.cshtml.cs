using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;

namespace TarlBreuJacoBaraKnor.webapp.Pages.Customer.Cart;

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
        // Get the current user's ID from the authentication cookie
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            // If user is not logged in, redirect to login page
            return RedirectToPage("/Identity/Login");
        }

        // Get the user's cart from the database
        var cartResponse = await _mediator.Send(new GetCartByUserId.Request(userId));

        if (!cartResponse.CartId.HasValue)
        {
            // If no cart exists, redirect back to cart page
            return RedirectToPage("/Customer/Cart/Cart");
        }

        // Load the full cart with items
        var cart = await _mediator.Send(new Get.Request(cartResponse.CartId.Value));

        if (cart is null || !cart.Items.Any())
        {
            // If cart is empty, show error message
            Errors = new[] { "Your cart is empty." };
            return Page();
        }

        // Calculate the total amount of the cart (sum of all items)
        var totalAmount = cart.Items.Sum(item => item.Sum);

        // Prepare delivery location details
        var location = new Location
        {
            Building = Building,
            RoomNumber = RoomNumber,
            Notes = Notes ?? ""
        };

        // Create a Stripe checkout session with total amount in NOK
        var checkoutUrl = _paymentService.CreatePaymentSession(totalAmount, "nok");

        // Store necessary order information in TempData for use after payment success
        TempData["CartId"] = cartResponse.CartId.Value.ToString();
        TempData["UserId"] = userId.ToString();
        TempData["Building"] = Building;
        TempData["RoomNumber"] = RoomNumber;
        TempData["Notes"] = Notes ?? "";
        TempData["TotalAmount"] = totalAmount.ToString();

        // Redirect the user to Stripe checkout page
        return Redirect(checkoutUrl);
    }
}