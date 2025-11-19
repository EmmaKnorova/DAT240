using Stripe.Checkout;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services
{
    public class StripePaymentService : IPaymentService
    {
        public string CreatePaymentSession(decimal amount, string currency = "nok")
        {
            var options = new SessionCreateOptions
            {
                Locale = "en",
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(amount * 100),
                            Currency = currency,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Order Payment"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = "http://localhost:8080/PaymentSuccess",
                CancelUrl = "http://localhost:8080/PaymentCancelled"
            };

            var service = new SessionService();
            var session = service.Create(options);

            return session.Url;
        }
    }
}