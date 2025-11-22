using System;
using System.Collections.Generic;
using System.Linq;
using TarlBreuJacoBaraKnor.webapp.SharedKernel;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart;

public class ShoppingCart : BaseEntity
{
	private ShoppingCart() { }
	public ShoppingCart(Guid id, Guid userId)
    {
        Id = id;
        UserId = userId;
    }

	public Guid Id { get; protected set; }
	public Guid? UserId { get; set; }

	private readonly List<CartItem> _items = new();
	public IEnumerable<CartItem> Items => _items.AsReadOnly();

	public void AddItem(int itemId, string itemName, decimal itemPrice)
	{
		var item = _items.SingleOrDefault(item => item.Sku == itemId);
		if (item == null)
		{
			item = new(itemId, itemName, itemPrice);
			_items.Add(item);
			return;
		}
		item.AddOne();
	}

	public void RemoveItem(int itemId)
    {
        var item = _items.SingleOrDefault(item => item.Sku == itemId);
        if (item == null)
        {
            throw new InvalidOperationException($"Item with SKU {itemId} not found in cart");
        }

        if (item.Count > 1)
        {
            item.RemoveOne();
        }
        else
        {
            _items.Remove(item);
        }
    }

	public void RemoveItemCompletely(int itemId)
    {
        var item = _items.SingleOrDefault(item => item.Sku == itemId);
        if (item == null)
        {
            throw new InvalidOperationException($"Item with SKU {itemId} not found in cart");
        }

        _items.Remove(item);
    }

    public void ClearCart()
    {
        _items.Clear();
    }

}
