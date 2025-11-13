using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.SharedKernel;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;

public class Order : BaseEntity
{   
    public Guid Id { get; set; }

    public DateTime OrderDate { get; set; }

    public List<OrderLine> OrderLines { get; set; } = new List<OrderLine>();

    public Location Location { get; set; }

    public string Notes { get; set; }
    public User Customer { get; set; }
    public Status Status { get; set; } = Status.Submitted;

    public Order()
    {
        Id = Guid.NewGuid();
    }

    public Order(Location location, User customer, string notes) : this()
    {
        Location = location;
        Customer = customer;
        Notes = notes;
    }
   
    public void AddOrderLine(OrderLine orderLine)
    {
        OrderLines.Add(orderLine);
    }
}
