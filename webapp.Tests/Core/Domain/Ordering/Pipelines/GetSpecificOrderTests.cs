using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace webapp.Tests.Core.Domain.Ordering.Pipelines
{
    public class GetSpecificOrderTests : IClassFixture<DbTest>
    {
        private readonly DbTest _dbTest;
        private readonly ITestOutputHelper _output;

        public GetSpecificOrderTests(DbTest dbTest, ITestOutputHelper output)
        {
            _dbTest = dbTest;
            _output = output;
        }

        [Fact]
        public async Task Handle_ReturnsOrderById()
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

            context.Set<Order>().Add(order);
            await context.SaveChangesAsync();


            var request = new GetSpecificOrder.Request(order.Id);

            var handler = new GetSpecificOrder.Handler(context);

            var response = await handler.Handle(request, CancellationToken.None);

            Assert.Equal(response.Id, order.Id);
            Assert.Equal(response.Customer.Id, user.Id);

        }
        [Fact]
        public async Task Handle_ThrowsKeyNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            await using var context = _dbTest.CreateContext();
            var nonExistentOrderId = Guid.NewGuid();

            var request = new GetSpecificOrder.Request(nonExistentOrderId);
            var handler = new GetSpecificOrder.Handler(context);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => handler.Handle(request, CancellationToken.None)
            );

            Assert.Equal($"Order with ID {nonExistentOrderId} not found.", exception.Message);
        }

    }
}