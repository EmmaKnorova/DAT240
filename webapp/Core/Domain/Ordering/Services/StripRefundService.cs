using Stripe;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services
{
    public class StripeRefundService
    {
        public async Task Refund(string paymentIntentId, decimal amount)
        {
            if (string.IsNullOrEmpty(paymentIntentId))
                throw new ArgumentException("PaymentIntentId cannot be null or empty.");

            // Stripe expects amounts in the smallest currency unit (øre for NOK)
            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Amount = (long)(amount * 100) // convert NOK to øre
            };

            var service = new RefundService();
            await service.CreateAsync(options);
        }
    }
}