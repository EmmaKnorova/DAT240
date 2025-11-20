
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Dto;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;

public interface IOrderingService
{
    Task<Guid> PlaceOrder(Location location, User user, OrderLineDto[] orderLines, string Notes, decimal deliveryFee, string paymentIntentId);
}

