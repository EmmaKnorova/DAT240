using TarlBreuJacoBaraKnor.webapp.SharedKernel;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Events;

public record OrderSent : BaseDomainEvent
{

    public OrderSent(Guid id)
    {
        orderId = id;
    }

    public Guid orderId { get; protected set; }
}   