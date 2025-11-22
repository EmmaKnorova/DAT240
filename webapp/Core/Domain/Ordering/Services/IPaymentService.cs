using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services
{
    public interface IPaymentService
    {
       (string Url, string PaymentIntentId) CreatePaymentSession(ShoppingCart cart, decimal deliveryFee, string currency = "nok");
    }
}