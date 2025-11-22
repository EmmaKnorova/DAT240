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
        // Uses PaymentIntents as the source of truth for successful payments.
        // Assumes StripeConfiguration.ApiKey is already set in Program.cs.
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

            // Stripe amounts are in the smallest currency unit (e.g. cents)
            totalAmountCents += pi.Amount;

            // Optional: tip stored as metadata "tip_amount" in cents
            if (pi.Metadata != null &&
                pi.Metadata.TryGetValue("tip_amount", out var tipRaw) &&
                long.TryParse(tipRaw, out var tipCents))
            {
                totalTipsCents += tipCents;
            }
        }

        decimal gross = totalAmountCents / 100m;
        decimal tips = totalTipsCents / 100m;

        // If you want exact Stripe fees, you can later use BalanceTransactionService.
        // For now, we keep fees at 0 and treat gross = net.
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