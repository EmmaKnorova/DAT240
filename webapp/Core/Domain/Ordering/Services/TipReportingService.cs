using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Stripe;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;

public class TipReportingService : ITipReportingService
{
    private readonly ShopContext _db;
    private readonly PaymentIntentService _paymentIntentService;

    public TipReportingService(ShopContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _paymentIntentService = new PaymentIntentService();
    }

    public async Task<decimal> GetTipsForPeriodAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        var fromUtc = from.UtcDateTime;
        var toUtc = to.UtcDateTime;

        var tipPaymentIntentIds = await _db.Orders
            .Where(o => o.OrderDate >= fromUtc && o.OrderDate < toUtc)
            .Where(o => !string.IsNullOrWhiteSpace(o.TipPaymentIntentId))
            .Select(o => o.TipPaymentIntentId!)
            .Distinct()
            .ToListAsync(cancellationToken);

        decimal totalTips = 0m;

        foreach (var tipPiId in tipPaymentIntentIds)
        {
            if (string.IsNullOrWhiteSpace(tipPiId))
                continue;

            var pi = await _paymentIntentService.GetAsync(
                tipPiId,
                cancellationToken: cancellationToken
            );

            var tipAmount = pi.Amount / 100m;
            totalTips += tipAmount;
        }

        return totalTips;
    }
}