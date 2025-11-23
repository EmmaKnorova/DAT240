using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Stripe;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;

public class StripeFinancialReportingService : IFinancialReportingService
{
    private readonly ShopContext _db;
    private readonly PaymentIntentService _paymentIntentService;

    public StripeFinancialReportingService(ShopContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _paymentIntentService = new PaymentIntentService();
    }

    public async Task<MonthlyFinancialSummary> GetMonthlySummaryAsync(
        DateTimeOffset from,
        DateTimeOffset to)
    {
        var listOptions = new PaymentIntentListOptions
        {
            Limit = 100,
            Created = new DateRangeOptions
            {
                GreaterThanOrEqual = from.UtcDateTime,
                LessThan = to.UtcDateTime
            }
        };

        var list = await _paymentIntentService.ListAsync(listOptions);

        long totalAmountCents = 0;
        int successfulCount = 0;

        foreach (var pi in list.Data)
        {
            if (!string.Equals(pi.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
                continue;

            successfulCount++;
            totalAmountCents += pi.Amount;
        }

        decimal gross = totalAmountCents / 100m;

        decimal fees = 0m;
        decimal net = gross - fees;

        decimal courierShare = Math.Round(net * 0.80m, 2, MidpointRounding.AwayFromZero);
        decimal serviceShare = net - courierShare;

        var start = from.UtcDateTime;
        var end = to.UtcDateTime;

        var tipPaymentIntentIds = await _db.Orders
            .Where(o =>
                o.OrderDate >= start &&
                o.OrderDate < end &&
                !string.IsNullOrWhiteSpace(o.TipPaymentIntentId))
            .Select(o => o.TipPaymentIntentId)
            .Distinct()
            .ToListAsync();

        long totalTipsCents = 0;

        foreach (var tipPiId in tipPaymentIntentIds)
        {
            try
            {
                var tipPi = await _paymentIntentService.GetAsync(tipPiId);

                if (string.Equals(tipPi.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
                {
                    totalTipsCents += tipPi.Amount;
                }
            }
            catch (StripeException) {}
        }

        decimal tips = totalTipsCents / 100m;

        return new MonthlyFinancialSummary(
            TotalGross: gross,
            StripeFees: fees,
            NetAfterFees: net,
            CourierShare: courierShare,
            ServiceShare: serviceShare,
            TotalTips: tips,
            SuccessfulPaymentsCount: successfulCount
        );
    }
}