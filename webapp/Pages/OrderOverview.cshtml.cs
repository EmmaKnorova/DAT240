using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.webapp.Pages
{
    public class OrderOverviewModel : PageModel
    {

        public List<Order> Orders { get; set; } = new();
        private readonly IMediator _mediator;
        private readonly UserManager<User> _userManager;

        public OrderOverviewModel(IMediator mediator, UserManager<User> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }
        public async Task OnGetAsync()
        {
            User User = await _userManager.GetUserAsync(HttpContext.User);
            Orders = await _mediator.Send(new Core.Domain.Ordering.Pipelines.Get.Request(User.Id));
        }
        public IActionResult OnPost(Guid orderId)
        {
            return RedirectToPage("/OrderDetail", new { id = orderId });
        }
    }
}
