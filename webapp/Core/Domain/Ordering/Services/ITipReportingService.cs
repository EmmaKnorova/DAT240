using System;
using System.Threading;
using System.Threading.Tasks;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;

public interface ITipReportingService
{
    Task<decimal> GetTipsForPeriodAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);
}
