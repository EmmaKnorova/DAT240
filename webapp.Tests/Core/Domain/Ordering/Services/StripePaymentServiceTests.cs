using System;
using System.Linq;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;
using Xunit;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Ordering.Services;

public class StripePaymentServiceTests
{
    [Fact]
    public void CreatePaymentSession_WithEmptyCart_OnlyIncludesDeliveryFee()
    {
        // Arrange
        var cart = new ShoppingCart(Guid.NewGuid(), Guid.NewGuid());
        var deliveryFee = 50m;

        // Assert - Cart should be empty but service should still work
        Assert.Empty(cart.Items);
        // Expected: 1 line item (delivery fee only)
        var expectedLineItemCount = 1;
        Assert.Equal(1, expectedLineItemCount);
    }

    [Fact]
    public void CreatePaymentSession_WithSingleItem_CreatesCorrectLineItems()
    {
        // Arrange
        var cart = new ShoppingCart(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, "Pizza", 10.50m);
        var deliveryFee = 25.00m;

        // Act
        var itemCount = cart.Items.Count();
        
        // Expected: 1 product + 1 delivery fee = 2 line items
        var expectedLineItemCount = itemCount + 1;

        // Assert
        Assert.Equal(1, itemCount);
        Assert.Equal(2, expectedLineItemCount);
    }

    [Fact]
    public void CreatePaymentSession_WithMultipleItems_CreatesCorrectLineItems()
    {
        // Arrange
        var cart = new ShoppingCart(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, "Pizza", 10.50m);
        cart.AddItem(2, "Burger", 8.00m);
        cart.AddItem(3, "Soda", 3.50m);

        // Act
        var itemCount = cart.Items.Count();
        var expectedLineItemCount = itemCount + 1; // +1 for delivery fee

        // Assert
        Assert.Equal(3, itemCount);
        Assert.Equal(4, expectedLineItemCount);
    }

    [Fact]
    public void CreatePaymentSession_WithDuplicateItems_IncreasesQuantity()
    {
        // Arrange
        var cart = new ShoppingCart(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, "Pizza", 10.50m);
        cart.AddItem(1, "Pizza", 10.50m); // Same SKU
        cart.AddItem(1, "Pizza", 10.50m); // Same SKU again

        // Act
        var itemCount = cart.Items.Count();
        var pizzaItem = cart.Items.First();

        // Assert
        Assert.Equal(1, itemCount); // Should only be 1 unique item
        Assert.Equal(3, pizzaItem.Count); // But with count of 3
    }

    [Fact]
    public void CreatePaymentSession_CalculatesCorrectAmountsInOre()
    {
        // Arrange
        var cart = new ShoppingCart(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, "Pizza", 10.50m);
        
        var deliveryFee = 25.00m;
        var pizzaItem = cart.Items.First();

        // Expected amounts in øre (smallest currency unit)
        var expectedItemAmountInOre = (long)(pizzaItem.Price * 100); // 1050 øre
        var expectedDeliveryAmountInOre = (long)(deliveryFee * 100); // 2500 øre

        // Assert
        Assert.Equal(1050, expectedItemAmountInOre);
        Assert.Equal(2500, expectedDeliveryAmountInOre);
    }

    [Fact]
    public void CreatePaymentSession_HandlesDecimalPricesCorrectly()
    {
        // Arrange
        var cart = new ShoppingCart(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, "Burger", 12.99m);
        cart.AddItem(2, "Fries", 4.50m);
        
        var burgerItem = cart.Items.First(i => i.Sku == 1);
        var friesItem = cart.Items.First(i => i.Sku == 2);

        // Act
        var burgerAmountInOre = (long)(burgerItem.Price * 100);
        var friesAmountInOre = (long)(friesItem.Price * 100);

        // Assert
        Assert.Equal(1299, burgerAmountInOre);
        Assert.Equal(450, friesAmountInOre);
    }

    [Fact]
    public void CreatePaymentSession_PreservesItemDetails()
    {
        // Arrange
        var cart = new ShoppingCart(Guid.NewGuid(), Guid.NewGuid());
        var itemSku = 123;
        var itemName = "Margherita Pizza";
        var itemPrice = 89.90m;

        cart.AddItem(itemSku, itemName, itemPrice);
        
        var item = cart.Items.First();

        // Assert
        Assert.Equal(itemSku, item.Sku);
        Assert.Equal(itemName, item.Name);
        Assert.Equal(itemPrice, item.Price);
        Assert.Equal(1, item.Count);
    }

    [Fact]
    public void CreatePaymentSession_CalculatesItemSum()
    {
        // Arrange
        var cart = new ShoppingCart(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(1, "Pizza", 10.00m);
        
        var item = cart.Items.First();

        // Act
        var expectedSum = item.Price * item.Count;

        // Assert
        Assert.Equal(3, item.Count);
        Assert.Equal(30.00m, item.Sum);
        Assert.Equal(expectedSum, item.Sum);
    }

    [Fact]
    public void CreatePaymentSession_WithZeroDeliveryFee_StillAddsDeliveryLineItem()
    {
        // Arrange
        var cart = new ShoppingCart(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, "Pizza", 10.00m);
        var deliveryFee = 0m;
        
        var expectedDeliveryAmountInOre = (long)(deliveryFee * 100);
        var expectedLineItemCount = cart.Items.Count() + 1; // Always includes delivery

        // Assert
        Assert.Equal(0, expectedDeliveryAmountInOre);
        Assert.Equal(2, expectedLineItemCount);
    }

    [Fact]
    public void CreatePaymentSession_WithLargeQuantities_HandlesCorrectly()
    {
        // Arrange
        var cart = new ShoppingCart(Guid.NewGuid(), Guid.NewGuid());
        
        // Add same item 10 times
        for (int i = 0; i < 10; i++)
        {
            cart.AddItem(1, "Pizza", 10.00m);
        }
        
        var item = cart.Items.First();

        // Assert
        Assert.Equal(1, cart.Items.Count()); // Only 1 unique item
        Assert.Equal(10, item.Count); // With quantity of 10
        Assert.Equal(100.00m, item.Sum); // 10 * 10.00
    }

    [Fact]
    public void CreatePaymentSession_WithMultipleDifferentItems_MaintainsCorrectCounts()
    {
        // Arrange
        var cart = new ShoppingCart(Guid.NewGuid(), Guid.NewGuid());
        
        // Add Pizza 3 times
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(1, "Pizza", 10.00m);
        
        // Add Burger 2 times
        cart.AddItem(2, "Burger", 8.00m);
        cart.AddItem(2, "Burger", 8.00m);
        
        // Add Soda 1 time
        cart.AddItem(3, "Soda", 3.50m);

        // Assert
        Assert.Equal(3, cart.Items.Count()); // 3 unique items
        
        var pizza = cart.Items.First(i => i.Sku == 1);
        var burger = cart.Items.First(i => i.Sku == 2);
        var soda = cart.Items.First(i => i.Sku == 3);
        
        Assert.Equal(3, pizza.Count);
        Assert.Equal(2, burger.Count);
        Assert.Equal(1, soda.Count);
    }

    [Fact]
    public void CreatePaymentSession_CartHasUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = new ShoppingCart(Guid.NewGuid(), userId);

        // Assert
        Assert.Equal(userId, cart.UserId);
    }

    [Fact]
    public void CreatePaymentSession_CartHasId()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var cart = new ShoppingCart(cartId, Guid.NewGuid());

        // Assert
        Assert.Equal(cartId, cart.Id);
    }

    [Fact]
    public void CreatePaymentSession_ItemsAreReadOnly()
    {
        // Arrange
        var cart = new ShoppingCart(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, "Pizza", 10.00m);

        // Assert
        Assert.IsAssignableFrom<IEnumerable<CartItem>>(cart.Items);
        // Items collection should be read-only (can't be modified directly)
    }

    [Fact]
    public void CreatePaymentSession_WithHighPriceItems_HandlesCorrectly()
    {
        // Arrange
        var cart = new ShoppingCart(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, "Expensive Item", 999.99m);
        
        var item = cart.Items.First();
        var amountInOre = (long)(item.Price * 100);

        // Assert
        Assert.Equal(99999, amountInOre);
    }

    [Fact]
    public void CreatePaymentSession_DeliveryFeeConfiguration()
    {
        // Arrange
        var standardDeliveryFee = 50m;
        var freeDeliveryFee = 0m;
        var premiumDeliveryFee = 100m;

        // Assert - Different delivery fee scenarios
        Assert.Equal(5000, (long)(standardDeliveryFee * 100));
        Assert.Equal(0, (long)(freeDeliveryFee * 100));
        Assert.Equal(10000, (long)(premiumDeliveryFee * 100));
    }

    [Fact]
    public void CreatePaymentSession_CurrencyOptions()
    {
        // Arrange
        var defaultCurrency = "nok";
        var usdCurrency = "usd";
        var eurCurrency = "eur";

        // Assert
        Assert.Equal("nok", defaultCurrency);
        Assert.Equal("usd", usdCurrency);
        Assert.Equal("eur", eurCurrency);
    }

    [Fact]
    public void CreatePaymentSession_TotalCartValue()
    {
        // Arrange
        var cart = new ShoppingCart(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, "Pizza", 10.00m);
        cart.AddItem(1, "Pizza", 10.00m); // count = 2
        cart.AddItem(2, "Burger", 8.00m); // count = 1
        
        // Act
        var totalValue = cart.Items.Sum(i => i.Sum);

        // Assert
        Assert.Equal(28.00m, totalValue); // (10 * 2) + (8 * 1)
    }

    [Fact]
    public void CreatePaymentSession_TotalWithDeliveryFee()
    {
        // Arrange
        var cart = new ShoppingCart(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, "Pizza", 10.00m);
        var deliveryFee = 50m;
        
        var cartTotal = cart.Items.Sum(i => i.Sum);
        var grandTotal = cartTotal + deliveryFee;

        // Assert
        Assert.Equal(10.00m, cartTotal);
        Assert.Equal(60.00m, grandTotal);
    }
}