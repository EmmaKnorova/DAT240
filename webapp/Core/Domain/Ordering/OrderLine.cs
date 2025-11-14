using TarlBreuJacoBaraKnor.webapp.SharedKernel;


namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;

public class OrderLine : BaseEntity
{
    public Guid Id { get; set; }
    public Guid FoodItemId { get; set; }
    public string FoodItemName { get; set; }
    public int Amount { get; set; }
    public decimal Price { get; set; }

    public OrderLine()
    {
        Id = Guid.NewGuid();
    }

    public OrderLine(Guid foodItemId, string foodItemName, int amount, decimal price)
    {
        FoodItemId = foodItemId;
        FoodItemName = foodItemName;
        Amount = amount;
        Price = price;
    }



}