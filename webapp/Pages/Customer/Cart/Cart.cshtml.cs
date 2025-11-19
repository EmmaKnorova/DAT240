using System.Security.Claims;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace TarlBreuJacoBaraKnor.webapp.Pages.Customer.Cart;

public class CartModel : PageModel
{
    private readonly IMediator _mediator;

    public CartModel(IMediator mediator) => _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));

    public ShoppingCart? Cart { get; private set; }

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

        if (cartResponse.CartId.HasValue)
        {
            Cart = await _mediator.Send(new Get.Request(cartResponse.CartId.Value));
        }

        return Page();
    }
}