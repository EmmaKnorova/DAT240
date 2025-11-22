using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;

public interface IFinancialAnalyticsService
{
    Task<FinancialDashboardData> GetDashboardDataAsync(
        DateTimeOffset from,
        DateTimeOffset to);
}

public record FinancialTimePoint(
    DateTime Date,
    decimal Gross,
    decimal Net,
    int NewCustomers,
    int SuccessfulPayments,
    int FailedPayments
);

public record FinancialDashboardData(
    IReadOnlyList<FinancialTimePoint> Daily
);