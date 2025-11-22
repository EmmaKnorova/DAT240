using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products.Pipelines;
using TarlBreuJacoBaraKnor.Pages.Admin.Helpers;

namespace TarlBreuJacoBaraKnor.Pages.Admin.Products;
[ServiceFilter(typeof(RequireChangingPasswordFilter))]
[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public List<FoodItem> Products { get; private set; } = new List<FoodItem>();

    public async Task OnGetAsync()
    {
        Products = await _mediator.Send(new Get.Request());
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            await _mediator.Send(new Delete.Request(id));
            TempData["SuccessMessage"] = "Product deleted successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }
}
