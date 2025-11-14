using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.webapp.Pages
{
    public class OrderOverviewModel : PageModel
    {

        public List<Order> ActiveOrders { get; set; } = new();
        public List<Order> PastOrders { get; set; } = new();
        private readonly IMediator _mediator;
        private readonly UserManager<User> _userManager;
        private User User;

        public OrderOverviewModel(IMediator mediator, UserManager<User> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }
        public async Task OnGetAsync()
        {
            User = await _userManager.GetUserAsync(HttpContext.User);
            var orders = await _mediator.Send(new Get.Request(User.Id));

            ActiveOrders = orders
            .Where(o => o.Status == Status.Submitted || o.Status == Status.Being_picked_up || o.Status == Status.On_the_way)
            .ToList();

            PastOrders = orders
            .Where(o => o.Status == Status.Delivered)
            .ToList();
        }
        public IActionResult OnPost(Guid orderId)
        {
            return RedirectToPage("/OrderDetail", new { id = orderId });
        }
    }
}
