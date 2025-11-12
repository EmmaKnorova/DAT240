using TarlBreuJacoBaraKnor.webapp.SharedKernel;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Products.Events;

public record FoodItemPriceChanged : BaseDomainEvent
{
	public FoodItemPriceChanged(int itemId, decimal oldPrice, decimal newPrice)
	{
		ItemId = itemId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
	}

	public int ItemId { get; }
	public decimal OldPrice { get; }
	public decimal NewPrice { get; }
}
