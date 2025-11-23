using MediatR;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.DTOs;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;

public class OrderingService : IOrderingService
{
    private readonly IMediator _mediator;

    public OrderingService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Guid> PlaceOrder(Location location, User User, OrderLineDto[] orderLines, string notes, decimal deliveryFee, string paymentIntentId)
    {

        var order = new Order(location, User, notes)
        {
            OrderDate = DateTimeOffset.UtcNow,
            Status = Status.Submitted,
            DeliveryFee = deliveryFee,
            PaymentIntentId = paymentIntentId
        };

        foreach (var line in orderLines)
            order.AddOrderLine(new OrderLine(Guid.NewGuid(), line.FoodItemName, line.Amount, line.Price));

        order.Events.Add(new OrderPlaced(order.Id));

        await _mediator.Send(new AddOrder.Request(order));
        await _mediator.Send(new SaveChanges.Request());

        return order.Id;
    }
}
