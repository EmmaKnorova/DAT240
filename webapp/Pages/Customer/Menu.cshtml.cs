using System.Security.Claims;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart.Pipelines;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace TarlBreuJacoBaraKnor.webapp.Pages.Customer;
[Authorize(Roles = "Customer")]
public class MenuModel : PageModel
{
    private readonly IMediator _mediator;
    public MenuModel(IMediator mediator) => _mediator = mediator;

    public List<FoodItem> FoodItems { get; set; } = new();

    public async Task OnGetAsync()
        => FoodItems = await _mediator.Send(new Core.Domain.Products.Pipelines.Get.Request());

    public async Task<IActionResult> OnPostAddToCartAsync(int id, string name, decimal price)
    {
        // Get the current user's ID from the authentication cookie
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return RedirectToPage("/Identity/Login");
        }

        // Get the user's cart from the database (or create one if it doesn't exist)
        var cartResponse = await _mediator.Send(new GetCartByUserId.Request(userId));
        var cartId = cartResponse.CartId ?? Guid.NewGuid();

        // Add item to cart
        await _mediator.Send(new AddItem.Request(id, name, price, userId, cartId));

        return RedirectToPage();
    }
}