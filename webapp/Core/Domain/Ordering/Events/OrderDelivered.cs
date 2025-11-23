using TarlBreuJacoBaraKnor.webapp.SharedKernel;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Events;

public record OrderDelivered : BaseDomainEvent
{
    public OrderDelivered(Guid id)
    {
        orderId = id;
    }

    public Guid orderId { get; protected set; }
}