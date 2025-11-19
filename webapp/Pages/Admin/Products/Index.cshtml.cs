using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products.Pipelines;

namespace TarlBreuJacoBaraKnor.Pages.Admin.Products;

[Authorize(Roles = "Admin")]
public class IndexModel(IMediator mediator) : PageModel
{
    private readonly IMediator _mediator = mediator;

    public List<FoodItem> Products { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Products = await _mediator.Send(new Get.Request());
    }
}
