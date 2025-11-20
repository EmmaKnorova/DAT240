using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TarlBreuJacoBaraKnor.Pages.Admin.Helpers;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;

namespace TarlBreuJacoBaraKnor.Pages.Admin;

[ServiceFilter(typeof(RequireChangingPasswordFilter))]
[Authorize(Roles = "Admin")]
public class AdminDashboardModel(IMediator mediator, ILogger<AdminDashboardModel> logger) : PageModel
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<AdminDashboardModel> _logger = logger;

    public int TotalOrdersThisMonth { get; private set; }
    public decimal TotalRevenueThisMonth { get; private set; }
    public IReadOnlyCollection<GetMonthlyOrderStats.StatusCountDto> OrdersByStatusThisMonth { get; private set; }
        = Array.Empty<GetMonthlyOrderStats.StatusCountDto>();

    public async Task OnGetAsync()
    {
        var now = DateTimeOffset.UtcNow;

        var stats = await _mediator.Send(
            new GetMonthlyOrderStats.Request(now.Year, now.Month)
        );

        TotalOrdersThisMonth = stats.TotalOrders;
        TotalRevenueThisMonth = stats.TotalRevenue;
        OrdersByStatusThisMonth = stats.OrdersByStatus;

        _logger.LogInformation(
            "Loaded admin dashboard stats for {Year}-{Month}: {TotalOrders} orders, {Revenue} revenue",
            now.Year, now.Month, TotalOrdersThisMonth, TotalRevenueThisMonth
        );
    }
}