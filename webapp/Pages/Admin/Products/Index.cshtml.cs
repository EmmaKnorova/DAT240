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
public class IndexModel(IMediator mediator) : PageModel
{
    private readonly IMediator _mediator = mediator;

    public List<FoodItem> Products { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Products = await _mediator.Send(new Get.Request());
    }
}
