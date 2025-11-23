using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TarlBreuJacoBaraKnor.webapp.Pages.Courier.Helpers;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;
using TarlBreuJacoBaraKnor.Pages.Admin.Helpers;
using Microsoft.AspNetCore.Identity;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.webapp.Pages.Courier;

[ServiceFilter(typeof(RequireChangingPasswordFilter))]
[Authorize(Roles = "Courier")]
public class EarningsOverviewModel(IMediator mediator, ILogger<EarningsOverviewModel> logger, UserManager<User> userManager) : PageModel
{
    private readonly IMediator _mediator = mediator;
    private readonly UserManager<User> _userManager = userManager;
    private readonly ILogger<EarningsOverviewModel> _logger = logger;
    public User Courier;

    public int TotalOrdersThisMonth { get; private set; }
    public decimal TotalRevenueThisMonth { get; private set; }  
    public decimal RevenueDelivery {get; private set;}
    public decimal RevenueTips {get; private set;}

    public int TotalOrders {get; private set;}
    public List<GetCourierEarningsByMonth.MonthlyEarningsDto> MonthlyEarnings { get; private set; } = [];


    public async Task OnGetAsync()
    {   
        Courier = await _userManager.GetUserAsync(HttpContext.User);
        var now = DateTimeOffset.UtcNow;

        var stats = await _mediator.Send(
            new GetMonthlyCourierStats.Request(now.Year, now.Month, Courier.Id)
        );

        MonthlyEarnings = await _mediator.Send(
            new GetCourierEarningsByMonth.Request(Courier.Id, now.Year)
        );

        TotalOrders = stats.TotalOrders;
        RevenueDelivery = stats.TotalRevenue;
        RevenueTips = stats.TipRevenue;
        


        _logger.LogInformation(
            "Loaded courier dashboard stats for {Year}-{Month}: {TotalOrders} orders, {Revenue} revenue",
            now.Year, now.Month, TotalOrdersThisMonth, TotalRevenueThisMonth
        );
    }
}