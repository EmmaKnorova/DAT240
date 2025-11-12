using TarlBreuJacoBaraKnor.webapp.SharedKernel;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Products.Events;


public record FoodItemNameChanged : BaseDomainEvent
{
	public FoodItemNameChanged(int itemId, string oldName, string newName)
	{
		ItemId = itemId;
		OldName = oldName ?? throw new System.ArgumentNullException(nameof(oldName));
		NewName = newName ?? throw new System.ArgumentNullException(nameof(newName));
	}

	public int ItemId { get; }
	public string OldName { get; }
	public string NewName { get; }
}
