using TarlBreuJacoBaraKnor.webapp.SharedKernel;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Events;

public record OrderAccepted : BaseDomainEvent
{
    public OrderAccepted(Guid id)
    {
        orderId = id;
    }

    public Guid orderId { get; protected set; }
}