using System.Security.Claims;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace TarlBreuJacoBaraKnor.webapp.Pages.Customer.Cart;

[Authorize(Roles = "Customer")]
public class CartModel : PageModel
{
    private readonly IMediator _mediator;

    public CartModel(IMediator mediator) => _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));

    public ShoppingCart? Cart { get; private set; }
    public IEnumerable<CartItem>? OrderedItems => Cart?.Items.OrderBy(i => i.Name);

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

    public async Task<IActionResult> OnPostRemoveItemAsync(Guid cartId, int itemSku)
    {
        var command = new RemoveItem.Request(cartId, itemSku);
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = "Item quantity decreased";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveItemCompletelyAsync(Guid cartId, int itemSku)
    {
        var command = new RemoveItemCompletely.Request(cartId, itemSku);
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = "Item removed from cart";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostClearCartAsync(Guid cartId)
    {
        var command = new ClearCart.Request(cartId);
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = "Cart cleared successfully";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddItemAsync(Guid cartId, int itemSku)
    {
        var cart = await _mediator.Send(new Get.Request(cartId));
        
        if (cart == null)
        {
            TempData["ErrorMessage"] = "Cart not found";
            return RedirectToPage();
        }

        var item = cart.Items.FirstOrDefault(i => i.Sku == itemSku);
        
        if (item == null)
        {
            TempData["ErrorMessage"] = "Item not found in cart";
            return RedirectToPage();
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return RedirectToPage("/Identity/Login");
        }

        var request = new AddItem.Request(
            itemSku,
            item.Name,
            item.Price,
            userId,
            cartId
        );

        await _mediator.Send(request);
        TempData["SuccessMessage"] = "Item quantity increased";

        return RedirectToPage();
    }
}