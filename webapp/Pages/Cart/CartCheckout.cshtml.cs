using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;

namespace TarlBreuJacoBaraKnor.webapp.Pages.Cart;

public class CartCheckoutModel : PageModel
{
    private readonly IMediator _mediator;

    public CartCheckoutModel(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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
            return RedirectToPage("/Cart/Cart");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
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
            return RedirectToPage("/Cart/Cart");
        }

        var location = new Location
        {
            Building = Building,
            RoomNumber = RoomNumber,
            Notes = Notes ?? ""
        };

        var result = await _mediator.Send(new CartCheckout.Request(
            cartResponse.CartId.Value, 
            location, 
            userId, 
            Notes ?? ""
        ));

        if (result.success)
        {
            return RedirectToPage("/OrderOverview");
        }

        Errors = result.Errors;
        return Page();
    } 
}