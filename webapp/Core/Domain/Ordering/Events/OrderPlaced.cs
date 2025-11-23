using TarlBreuJacoBaraKnor.webapp.SharedKernel;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Events;

public record OrderPlaced : BaseDomainEvent
{

    public OrderPlaced(Guid id)
    {
        orderId = id;
    }

    public Guid orderId { get; protected set; }
}   