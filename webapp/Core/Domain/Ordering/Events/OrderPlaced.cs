using TarlBreuJacoBaraKnor.webapp.SharedKernel;

public record OrderPlaced : BaseDomainEvent
{

    public OrderPlaced(Guid id)
    {
        orderId = id;
    }

    public Guid orderId { get; protected set; }
}   