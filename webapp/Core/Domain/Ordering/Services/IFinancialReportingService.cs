using System;
using System.Threading.Tasks;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;

public interface IFinancialReportingService
{
    Task<MonthlyFinancialSummary> GetMonthlySummaryAsync(
        DateTimeOffset from,
        DateTimeOffset to);
}

public record MonthlyFinancialSummary(
    decimal TotalGross,
    decimal StripeFees,
    decimal NetAfterFees,
    decimal CourierShare,
    decimal ServiceShare,
    decimal TotalTips,
    int SuccessfulPaymentsCount
);
