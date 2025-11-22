using System;
using System.Threading.Tasks;
using Stripe;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;

public class StripeFinancialReportingService : IFinancialReportingService
{
    public async Task<MonthlyFinancialSummary> GetMonthlySummaryAsync(
        DateTimeOffset from,
        DateTimeOffset to)
    {
        var service = new PaymentIntentService();

        var options = new PaymentIntentListOptions
        {
            Limit = 100,
            Created = new DateRangeOptions
            {
                GreaterThanOrEqual = from.UtcDateTime,
                LessThan = to.UtcDateTime
            }
        };

        var list = await service.ListAsync(options);

        long totalAmountCents = 0;
        long totalTipsCents = 0;
        int successfulCount = 0;

        foreach (var pi in list.Data)
        {
            if (!string.Equals(pi.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            successfulCount++;

            totalAmountCents += pi.Amount;

            if (pi.Metadata != null &&
                pi.Metadata.TryGetValue("tip_amount", out var tipRaw) &&
                long.TryParse(tipRaw, out var tipCents))
            {
                totalTipsCents += tipCents;
            }
        }

        decimal gross = totalAmountCents / 100m;
        decimal tips = totalTipsCents / 100m;

        decimal fees = 0m;
        decimal net = gross - fees;

        decimal courierShare = Math.Round(net * 0.80m, 2, MidpointRounding.AwayFromZero);
        decimal serviceShare = net - courierShare;

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