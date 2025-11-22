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

[Authorize(Roles = "Customer")]
public class OrderOverviewModel : PageModel
{
    public List<Order> ActiveOrders { get; set; } = new();
    public List<Order> PastOrders { get; set; } = new();
    private readonly IMediator _mediator;
    private readonly UserManager<User> _userManager;
    private readonly StripeRefundService _refundService;
    private readonly StripePaymentService _paymentService;

    private User User;

    public OrderOverviewModel(IMediator mediator, UserManager<User> userManager, 
                          StripeRefundService refundService, 
                          StripePaymentService paymentService)
    {
        _mediator = mediator;
        _userManager = userManager;
        _refundService = refundService;
        _paymentService = paymentService;
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

            foreach (var line in order.OrderLines)
            {
                line.Price = 0;
            }
            order.DeliveryFee = 0;
            order.Status = Status.Cancelled;

            await _mediator.Send(new UpdateOrder.Request(order));

            TempData["SuccessMessage"] = "Your order has been cancelled and fully refunded.";
        }
        // Cancel after courier accepts but before pickup → refund items only
        else if (order.Status == Status.Being_picked_up)
        {
            var itemsAmount = order.OrderLines.Sum(l => l.Amount * l.Price);
            await _refundService.Refund(order.PaymentIntentId, itemsAmount);

            foreach (var line in order.OrderLines)
            {
                line.Price = 0;
            }
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

    public async Task<IActionResult> OnPostTipAsync(Guid orderId, int tipAmount)
    {
        User = await _userManager.GetUserAsync(HttpContext.User);
        var order = (await _mediator.Send(new Get.Request(User.Id)))
            .FirstOrDefault(o => o.Id == orderId);

        if (order == null || order.Status != Status.Delivered)
        {
            TempData["ErrorMessage"] = "Tips can only be added to delivered orders.";
            return RedirectToPage();
        }

        // ✅ Guard: empêcher un second tip
        if (!string.IsNullOrEmpty(order.TipPaymentIntentId))
        {
            TempData["ErrorMessage"] = "Tip already given for this order.";
            return RedirectToPage();
        }

        var (url, paymentIntentId) = _paymentService.CreateTipSession(tipAmount, orderId);

        // On ne stocke pas encore ici → on stockera dans TipSuccess après confirmation
        return Redirect(url);
    }


}