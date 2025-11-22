using Stripe.Checkout;
using System.Collections.Generic;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services
{
    public class StripePaymentService : IPaymentService
    {
        // Create a Stripe Checkout session including all cart items and the delivery fee
        public (string Url, string PaymentIntentId) CreatePaymentSession(ShoppingCart cart, decimal deliveryFee, string currency = "nok")
        {
            var lineItems = new List<SessionLineItemOptions>();

            // Add each item from the cart as a separate line in Stripe Checkout
            foreach (var item in cart.Items)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        // Stripe expects amounts in the smallest currency unit (øre for NOK)
                        UnitAmount = (long)(item.Price * 100),
                        Currency = currency,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            // Display the product name in the Stripe Checkout page
                            Name = item.Name
                        }
                    },
                    // Quantity of the item ordered
                    Quantity = item.Count
                });
            }

            // Add the delivery fee as a separate line item
            lineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(deliveryFee * 100),
                    Currency = currency,
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        // Display "Delivery Fee" in the Stripe Checkout page
                        Name = "Delivery Fee"
                    }
                },
                Quantity = 1
            });

            // Configure the Stripe Checkout session
            var options = new SessionCreateOptions
            {
                Locale = "en",
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = "http://localhost:8080/PaymentSuccess?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "http://localhost:8080/PaymentCancelled"
            };

            // Create the session using Stripe's API
            var service = new SessionService();
            var session = service.Create(options);
            var paymentIntentId = session.PaymentIntentId;

            // Return the URL where the user will be redirected to pay
            return (session.Url, paymentIntentId);
        }

        // Create a Stripe Checkout session for a tip (courier gratuity)
        public (string Url, string PaymentIntentId) CreateTipSession(decimal tipAmount, Guid orderId, string currency = "nok")
        {
            var lineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(tipAmount * 100),
                        Currency = currency,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Courier Tip"
                        }
                    },
                    Quantity = 1
                }
            };

            var options = new SessionCreateOptions
            {
                Locale = "en",
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = $"http://localhost:8080/TipSuccess?session_id={{CHECKOUT_SESSION_ID}}&orderId={orderId}",
                CancelUrl = "http://localhost:8080/TipCancelled"
            };

            var service = new SessionService();
            var session = service.Create(options);

            return (session.Url, session.PaymentIntentId);
        }
    }
}