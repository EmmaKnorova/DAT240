using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

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
    [Required(ErrorMessage = "Building is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Building must be between 2 and 100 characters")]
    public string Building { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "Room number is required")]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "Room number must be between 1 and 20 characters")]
    [RegularExpression(@"^[A-Za-z0-9\-\.]+$", ErrorMessage = "Room number can only contain letters, numbers, hyphens, and periods")]
    public string RoomNumber { get; set; } = "";

    [BindProperty]
    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }

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
        // Validate ModelState first
        if (!ModelState.IsValid)
        {
            Errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToArray();
            return Page();
        }

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

        // Trim and validate inputs
        Building = Building.Trim();
        RoomNumber = RoomNumber.Trim().ToUpperInvariant();
        Notes = Notes?.Trim();

        // Additional validation (redundant with data annotations, but ensures consistency)
        if (string.IsNullOrWhiteSpace(Building))
        {
            Errors = new[] { "Building is required" };
            return Page();
        }

        if (string.IsNullOrWhiteSpace(RoomNumber))
        {
            Errors = new[] { "Room number is required" };
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