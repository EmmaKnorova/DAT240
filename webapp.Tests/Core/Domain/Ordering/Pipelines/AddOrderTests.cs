using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Ordering.Pipelines;
public class AddOrderTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;
    private readonly ITestOutputHelper _output;

    public AddOrderTests(DbTest dbTest, ITestOutputHelper output)
    {
        _dbTest = dbTest;
        _output = output;
    }

    [Fact]
    public async Task CreatesNewOrder()
    {
        await using var context = _dbTest.CreateContext();

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            UserName = "test@example.com",
            Address = "123 Test St",
            City = "Test City",
            PostalCode = "12345"
        };
        context.Users.Add(user);

        var location = new Location { Building = "1", RoomNumber = "2", Notes = " " };

        await context.SaveChangesAsync();

        var order = new Order(location, user, "Test order notes");
        var request = new AddOrder.Request(order);

        var handler = new AddOrder.Handler(context);

        var response = await handler.Handle(request, CancellationToken.None);

        Assert.True(response.Success);
        Assert.Empty(response.Errors);

        var addedOrder = await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Location)
            .SingleOrDefaultAsync(o => o.Id == order.Id);

        Assert.NotNull(addedOrder);
        Assert.Equal(order.Id, addedOrder.Id);
        Assert.Equal(userId, addedOrder.Customer.Id);
        Assert.Equal("1", addedOrder.Location.Building);
    }
}
