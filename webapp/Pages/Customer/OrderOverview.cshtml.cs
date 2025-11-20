using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.webapp.Pages.Customer;

[Authorize]
public class OrderOverviewModel : PageModel
{
    public List<Order> ActiveOrders { get; set; } = new();
    public List<Order> PastOrders { get; set; } = new();
    private readonly IMediator _mediator;
    private readonly UserManager<User> _userManager;
    private readonly StripeRefundService _refundService;
    private User User;

    public OrderOverviewModel(IMediator mediator, UserManager<User> userManager, StripeRefundService refundService)
    {
        _mediator = mediator;
        _userManager = userManager;
        _refundService = refundService;
    }

    public async Task OnGetAsync()
    {
        User = await _userManager.GetUserAsync(HttpContext.User);
        var orders = await _mediator.Send(new Get.Request(User.Id));

        ActiveOrders = orders
            .Where(o => o.Status == Status.Submitted || o.Status == Status.Being_picked_up || o.Status == Status.On_the_way)
            .ToList();

        PastOrders = orders
            .Where(o => o.Status == Status.Delivered || o.Status == Status.Cancelled || o.Status == Status.CancelledWithFee)
            .ToList();
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid orderId)
    {
        User = await _userManager.GetUserAsync(HttpContext.User);
        var order = (await _mediator.Send(new Get.Request(User.Id)))
            .FirstOrDefault(o => o.Id == orderId);

        if (order == null)
            return RedirectToPage();

        // Cancel before courier accepts → full refund
        if (order.Status == Status.Submitted)
        {
            var fullAmount = order.OrderLines.Sum(l => l.Amount * l.Price) + order.DeliveryFee;
            await _refundService.Refund(order.PaymentIntentId, fullAmount);

            order.Status = Status.Cancelled;
            await _mediator.Send(new UpdateOrder.Request(order));

            TempData["SuccessMessage"] = "Your order has been cancelled and fully refunded.";
        }
        // Cancel after courier accepts but before pickup → refund items only
        else if (order.Status == Status.Being_picked_up)
        {
            var itemsAmount = order.OrderLines.Sum(l => l.Amount * l.Price);
            await _refundService.Refund(order.PaymentIntentId, itemsAmount);

            order.Status = Status.CancelledWithFee;
            await _mediator.Send(new UpdateOrder.Request(order));

            TempData["SuccessMessage"] = "Your order has been cancelled. The delivery fee is non-refundable.";
        }
        // Cancel after pickup → not allowed
        else if (order.Status == Status.On_the_way || order.Status == Status.Delivered)
        {
            TempData["ErrorMessage"] = "Cannot cancel after pickup.";
        }

        return RedirectToPage();
    }

    public IActionResult OnPostDetails(Guid orderId)
    {
        return RedirectToPage("/Customer/OrderDetail", new { id = orderId });
    }
}