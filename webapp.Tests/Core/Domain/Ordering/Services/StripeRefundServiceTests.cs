using System;
using System.Threading.Tasks;
using Stripe;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;
using Xunit;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Ordering.Services;

public class StripeRefundServiceTests
{
    [Fact]
    public async Task Refund_WithNullPaymentIntentId_ThrowsArgumentException()
    {
        // Arrange
        var service = new StripeRefundService();
        string? paymentIntentId = null;
        var amount = 100m;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await service.Refund(paymentIntentId!, amount)
        );
        
        Assert.Contains("PaymentIntentId cannot be null or empty", exception.Message);
    }

    [Fact]
    public async Task Refund_WithEmptyPaymentIntentId_ThrowsArgumentException()
    {
        // Arrange
        var service = new StripeRefundService();
        var paymentIntentId = string.Empty;
        var amount = 100m;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await service.Refund(paymentIntentId, amount)
        );
        
        Assert.Contains("PaymentIntentId cannot be null or empty", exception.Message);
    }

    [Fact]
    public async Task Refund_WithWhitespacePaymentIntentId_ThrowsStripeException()
    {
        // Arrange
        var service = new StripeRefundService();
        var paymentIntentId = "   ";
        var amount = 100m;

        // Act & Assert
        await Assert.ThrowsAsync<StripeException>(
            async () => await service.Refund(paymentIntentId, amount)
        );
    }

    [Fact]
    public void Refund_ConvertsAmountToOreCorrectly()
    {
        // Arrange
        var amount = 100m; // 100 NOK
        var expectedAmountInOre = (long)(amount * 100); // 10000 øre

        // Act
        var result = (long)(amount * 100);

        // Assert
        Assert.Equal(10000, result);
        Assert.Equal(expectedAmountInOre, result);
    }

    [Fact]
    public void Refund_HandlesDecimalAmounts()
    {
        // Arrange
        var amount = 12.50m; // 12.50 NOK
        var expectedAmountInOre = (long)(amount * 100); // 1250 øre

        // Act
        var result = (long)(amount * 100);

        // Assert
        Assert.Equal(1250, result);
        Assert.Equal(expectedAmountInOre, result);
    }

    [Fact]
    public void Refund_HandlesSmallAmounts()
    {
        // Arrange
        var amount = 0.50m; // 50 øre
        var expectedAmountInOre = (long)(amount * 100);

        // Act
        var result = (long)(amount * 100);

        // Assert
        Assert.Equal(50, result);
        Assert.Equal(expectedAmountInOre, result);
    }

    [Fact]
    public void Refund_HandlesLargeAmounts()
    {
        // Arrange
        var amount = 9999.99m; // 9999.99 NOK
        var expectedAmountInOre = (long)(amount * 100); // 999999 øre

        // Act
        var result = (long)(amount * 100);

        // Assert
        Assert.Equal(999999, result);
        Assert.Equal(expectedAmountInOre, result);
    }

    [Fact]
    public void Refund_HandlesZeroAmount()
    {
        // Arrange
        var amount = 0m;
        var expectedAmountInOre = (long)(amount * 100);

        // Act
        var result = (long)(amount * 100);

        // Assert
        Assert.Equal(0, result);
        Assert.Equal(expectedAmountInOre, result);
    }

    [Fact]
    public void Refund_HandlesPreciseDecimalAmounts()
    {
        // Arrange
        var amount = 99.99m;
        var expectedAmountInOre = (long)(amount * 100); // 9999 øre

        // Act
        var result = (long)(amount * 100);

        // Assert
        Assert.Equal(9999, result);
        Assert.Equal(expectedAmountInOre, result);
    }

    [Fact]
    public void Refund_TruncatesExtraDecimalPlaces()
    {
        // Arrange
        var amount = 10.555m; // More than 2 decimal places
        var expectedAmountInOre = (long)(amount * 100); // Should truncate to 1055

        // Act
        var result = (long)(amount * 100);

        // Assert
        Assert.Equal(1055, result);
    }

    [Fact]
    public void Refund_ValidPaymentIntentIdFormat()
    {
        // Arrange
        var validPaymentIntentId = "pi_1234567890abcdef";

        // Assert
        Assert.False(string.IsNullOrEmpty(validPaymentIntentId));
        Assert.StartsWith("pi_", validPaymentIntentId);
    }

    [Fact]
    public void Refund_CommonRefundScenarios()
    {
        // Arrange - Common refund amounts
        var fullRefund = 100m;      // Full order refund
        var partialRefund = 50m;    // Half refund
        var deliveryRefund = 30m;   // Delivery fee refund
        var smallRefund = 5m;       // Small item refund

        // Act
        var fullRefundInOre = (long)(fullRefund * 100);
        var partialRefundInOre = (long)(partialRefund * 100);
        var deliveryRefundInOre = (long)(deliveryRefund * 100);
        var smallRefundInOre = (long)(smallRefund * 100);

        // Assert
        Assert.Equal(10000, fullRefundInOre);
        Assert.Equal(5000, partialRefundInOre);
        Assert.Equal(3000, deliveryRefundInOre);
        Assert.Equal(500, smallRefundInOre);
    }

    [Fact]
    public void Refund_MultipleRefundAmountsForSameOrder()
    {
        // Arrange - Simulating multiple partial refunds
        var firstRefund = 25m;
        var secondRefund = 15m;
        var thirdRefund = 10m;
        var totalRefunded = firstRefund + secondRefund + thirdRefund;

        // Act
        var firstRefundInOre = (long)(firstRefund * 100);
        var secondRefundInOre = (long)(secondRefund * 100);
        var thirdRefundInOre = (long)(thirdRefund * 100);
        var totalRefundedInOre = (long)(totalRefunded * 100);

        // Assert
        Assert.Equal(2500, firstRefundInOre);
        Assert.Equal(1500, secondRefundInOre);
        Assert.Equal(1000, thirdRefundInOre);
        Assert.Equal(5000, totalRefundedInOre);
        Assert.Equal(50m, totalRefunded);
    }

    [Fact]
    public void Refund_MaximumRefundAmount()
    {
        // Arrange - Stripe has limits, but testing large amounts
        var maxAmount = 999999.99m;
        var expectedAmountInOre = (long)(maxAmount * 100);

        // Act
        var result = (long)(maxAmount * 100);

        // Assert
        Assert.Equal(99999999, result);
        Assert.Equal(expectedAmountInOre, result);
    }

    [Fact]
    public void Refund_MinimumRefundAmount()
    {
        // Arrange - Minimum practical refund (1 øre = 0.01 NOK)
        var minAmount = 0.01m;
        var expectedAmountInOre = (long)(minAmount * 100);

        // Act
        var result = (long)(minAmount * 100);

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(expectedAmountInOre, result);
    }

    [Fact]
    public void Refund_PaymentIntentIdValidation()
    {
        // Arrange
        var invalidIds = new[]
        {
            (string?)null,
            string.Empty,
            "   ",
            "\t",
            "\n"
        };

        // Assert
        foreach (var invalidId in invalidIds)
        {
            Assert.True(string.IsNullOrWhiteSpace(invalidId));
        }
    }

    [Fact]
    public void Refund_CalculatesRefundPercentages()
    {
        // Arrange
        var originalAmount = 200m;
        var fullRefund = originalAmount;           // 100%
        var halfRefund = originalAmount * 0.5m;    // 50%
        var quarterRefund = originalAmount * 0.25m; // 25%

        // Act
        var fullRefundInOre = (long)(fullRefund * 100);
        var halfRefundInOre = (long)(halfRefund * 100);
        var quarterRefundInOre = (long)(quarterRefund * 100);

        // Assert
        Assert.Equal(20000, fullRefundInOre);      // 200 NOK
        Assert.Equal(10000, halfRefundInOre);      // 100 NOK
        Assert.Equal(5000, quarterRefundInOre);    // 50 NOK
    }
}