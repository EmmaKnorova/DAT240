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
        _chargeService = new ChargeService();
        _customerService = new CustomerService();
    }

    public async Task<FinancialDashboardData> GetDashboardDataAsync(
        DateTimeOffset from,
        DateTimeOffset to)
    {
        var fromUtc = from.UtcDateTime;
        var toUtc = to.UtcDateTime;

        var dailyBuckets = new SortedDictionary<DateTime, DailyAccumulator>();

        var chargeOptions = new ChargeListOptions
        {
            Limit = 100,
            Created = new DateRangeOptions
            {
                GreaterThanOrEqual = fromUtc,
                LessThan = toUtc
            }
        };

        var charges = await _chargeService.ListAsync(chargeOptions);

        foreach (var charge in charges.Data)
        {
            var created = charge.Created;
            var day = created.Date;

            if (!dailyBuckets.TryGetValue(day, out var acc))
            {
                acc = new DailyAccumulator();
                dailyBuckets[day] = acc;
            }

            long amountCents = charge.Amount;

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

    private sealed class DailyAccumulator
    {
        public long GrossCents { get; set; }
        public long NetCents { get; set; }
        public int NewCustomers { get; set; }
        public int SuccessfulPayments { get; set; }
        public int FailedPayments { get; set; }
    }
}