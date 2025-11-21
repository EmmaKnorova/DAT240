using System;
using TarlBreuJacoBaraKnor.webapp.SharedKernel;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart;

public class CartItem : BaseEntity
{
	public CartItem(int sku, string name, decimal price)
	{
		Sku = sku; // Stock Keeping Unit
		Name = name;
		Price = price;
		Count = 1;
	}
	public Guid Id { get; protected set; }

	public int Sku { get; private set; }
	public string Name { get; set; }
	public decimal Price { get; set; }
	public decimal Sum => Price * Count;

	public int Count { get; private set; }

	public void AddOne() => Count++;

	public void RemoveOne()
    {
        if (Count <= 0)
        {
            throw new InvalidOperationException("Cannot remove from an item with count 0");
        }
        Count--;
    }
}
