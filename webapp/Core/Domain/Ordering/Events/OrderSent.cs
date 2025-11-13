using TarlBreuJacoBaraKnor.webapp.SharedKernel;

public record OrderSent : BaseDomainEvent
{

    public OrderSent(Guid id)
    {
        orderId = id;
    }

    public Guid orderId { get; protected set; }
}   