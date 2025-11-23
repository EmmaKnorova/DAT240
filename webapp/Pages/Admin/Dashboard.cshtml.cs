using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TarlBreuJacoBaraKnor.Pages.Admin.Helpers;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;

namespace TarlBreuJacoBaraKnor.Pages.Admin;

[ServiceFilter(typeof(RequireChangingPasswordFilter))]
[Authorize(Roles = "Admin")]
public class AdminDashboardModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminDashboardModel> _logger;
    private readonly IFinancialReportingService _financialReportingService;
    private readonly IFinancialAnalyticsService _financialAnalyticsService;

    public AdminDashboardModel(
        IMediator mediator,
        ILogger<AdminDashboardModel> logger,
        IFinancialReportingService financialReportingService,
        IFinancialAnalyticsService financialAnalyticsService)
    {
        _mediator = mediator;
        _logger = logger;
        _financialReportingService = financialReportingService;
        _financialAnalyticsService = financialAnalyticsService;
    }

    [BindProperty(SupportsGet = true)]
    public DateTime? From { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? To { get; set; }

    public DateTimeOffset CurrentRangeStart { get; private set; }
    public DateTimeOffset CurrentRangeEnd { get; private set; }
    public int TotalOrdersThisMonth { get; private set; }
    public decimal TotalRevenueThisMonth { get; private set; }
    public IReadOnlyCollection<GetMonthlyOrderStats.StatusCountDto> OrdersByStatusThisMonth { get; private set; }
        = Array.Empty<GetMonthlyOrderStats.StatusCountDto>();

    public int OpenOrdersTotal { get; private set; }
    public int BeingDeliveredOrdersTotal { get; private set; }
    public int DeliveredOrdersTotal { get; private set; }

    public int OpenOrdersMonthToDate { get; private set; }
    public int BeingDeliveredOrdersMonthToDate { get; private set; }
    public int DeliveredOrdersMonthToDate { get; private set; }

    public MonthlyFinancialSummary FinancialSummary { get; private set; } = default!;

    public FinancialDashboardData FinancialAnalytics { get; private set; } = default!;

    public async Task OnGetAsync()
    {
        var now = DateTimeOffset.UtcNow;

        var rangeStartDate = From?.Date ?? new DateTime(now.Year, now.Month, 1);
        var rangeEndDate = To?.Date.AddDays(1) ?? rangeStartDate.AddMonths(1);

        CurrentRangeStart = new DateTimeOffset(rangeStartDate, TimeSpan.Zero);
        CurrentRangeEnd = new DateTimeOffset(rangeEndDate, TimeSpan.Zero);

        var reference = rangeStartDate;
        var monthlyStats = await _mediator.Send(
            new GetMonthlyOrderStats.Request(reference.Year, reference.Month)
        );

        TotalOrdersThisMonth = monthlyStats.TotalOrders;
        TotalRevenueThisMonth = monthlyStats.TotalRevenue;
        OrdersByStatusThisMonth = monthlyStats.OrdersByStatus;

        _logger.LogInformation(
            "Loaded admin dashboard stats for {Year}-{Month}: {TotalOrders} orders, {Revenue} revenue",
            reference.Year, reference.Month, TotalOrdersThisMonth, TotalRevenueThisMonth
        );

        var totalStatusStats = await _mediator.Send(new GetOrderStatusTotals.Request());

        (OpenOrdersTotal, BeingDeliveredOrdersTotal, DeliveredOrdersTotal) =
            AggregateByStage(totalStatusStats.OrdersByStatus.Select(s => (s.Status, s.Count)));

        (OpenOrdersMonthToDate, BeingDeliveredOrdersMonthToDate, DeliveredOrdersMonthToDate) =
            AggregateByStage(OrdersByStatusThisMonth.Select(s => (s.Status, s.Count)));

        FinancialSummary = await _financialReportingService.GetMonthlySummaryAsync(
            CurrentRangeStart,
            CurrentRangeEnd
        );

        FinancialAnalytics = await _financialAnalyticsService.GetDashboardDataAsync(
            CurrentRangeStart,
            CurrentRangeEnd
        );
    }
    private static (int open, int delivering, int delivered) AggregateByStage(
        IEnumerable<(string Status, int Count)> items)
    {
        int open = 0;
        int delivering = 0;
        int delivered = 0;

        foreach (var (statusName, count) in items)
        {
            if (IsBeingDelivered(statusName))
            {
                delivering += count;
            }
            else if (IsDelivered(statusName))
            {
                delivered += count;
            }
            else
            {
                open += count;
            }
        }

        return (open, delivering, delivered);
    }

    private static bool IsBeingDelivered(string statusName)
    {
        return statusName.Equals(Status.On_the_way.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDelivered(string statusName)
    {
        return statusName.Contains("Delivered", StringComparison.OrdinalIgnoreCase)
               || statusName.Contains("Completed", StringComparison.OrdinalIgnoreCase);
    }
}