using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stripe;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;

public class StripeFinancialAnalyticsService : IFinancialAnalyticsService
{
    private readonly ChargeService _chargeService;
    private readonly CustomerService _customerService;

    public StripeFinancialAnalyticsService()
    {
        // StripeConfiguration.ApiKey is set in Program.cs
        _chargeService = new ChargeService();
        _customerService = new CustomerService();
    }

    public async Task<FinancialDashboardData> GetDashboardDataAsync(
        DateTimeOffset from,
        DateTimeOffset to)
    {
        var fromUtc = from.UtcDateTime;
        var toUtc = to.UtcDateTime;

        // We aggregate metrics per calendar day (UTC-based).
        var dailyBuckets = new SortedDictionary<DateTime, DailyAccumulator>();

        // 1) Charges = payments (gross, net, success/fail)
        var chargeOptions = new ChargeListOptions
        {
            Limit = 100, // increase or add paging if needed
            Created = new DateRangeOptions
            {
                GreaterThanOrEqual = fromUtc,
                LessThan = toUtc
            }
        };

        var charges = await _chargeService.ListAsync(chargeOptions);

        foreach (var charge in charges.Data)
        {
            // In this Stripe SDK version, Created is a non-nullable DateTime
            var created = charge.Created;
            var day = created.Date;

            if (!dailyBuckets.TryGetValue(day, out var acc))
            {
                acc = new DailyAccumulator();
                dailyBuckets[day] = acc;
            }

            // Stripe amounts are in the smallest currency unit (e.g. cents)
            long amountCents = charge.Amount;

            // For now, we treat net = gross (you can later refine with BalanceTransaction)
            acc.GrossCents += amountCents;
            acc.NetCents += amountCents;

            bool isSuccessful =
                charge.Paid == true &&
                !charge.Refunded &&
                string.Equals(charge.Status, "succeeded", StringComparison.OrdinalIgnoreCase);

            if (isSuccessful)
            {
                acc.SuccessfulPayments++;
            }
            else
            {
                acc.FailedPayments++;
            }
        }

        // 2) Customers = "new customers" per day
        var customerOptions = new CustomerListOptions
        {
            Limit = 100,
            Created = new DateRangeOptions
            {
                GreaterThanOrEqual = fromUtc,
                LessThan = toUtc
            }
        };

        var customers = await _customerService.ListAsync(customerOptions);

        foreach (var customer in customers.Data)
        {
            var created = customer.Created;
            var day = created.Date;

            if (!dailyBuckets.TryGetValue(day, out var acc))
            {
                acc = new DailyAccumulator();
                dailyBuckets[day] = acc;
            }

            acc.NewCustomers++;
        }

        // 3) Map aggregates -> FinancialTimePoint list
        var points = dailyBuckets
            .Select(kvp =>
            {
                var d = kvp.Key;
                var acc = kvp.Value;

                return new FinancialTimePoint(
                    Date: d,
                    Gross: acc.GrossCents / 100m,
                    Net: acc.NetCents / 100m,
                    NewCustomers: acc.NewCustomers,
                    SuccessfulPayments: acc.SuccessfulPayments,
                    FailedPayments: acc.FailedPayments
                );
            })
            .ToList();

        return new FinancialDashboardData(points);
    }

    // Internal accumulator per day
    private sealed class DailyAccumulator
    {
        public long GrossCents { get; set; }
        public long NetCents { get; set; }
        public int NewCustomers { get; set; }
        public int SuccessfulPayments { get; set; }
        public int FailedPayments { get; set; }
    }
}