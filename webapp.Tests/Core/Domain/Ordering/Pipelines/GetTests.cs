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
    public class GetTests : IClassFixture<DbTest>
    {
        private readonly DbTest _dbTest;
        private readonly ITestOutputHelper _output;

        public GetTests(DbTest dbTest, ITestOutputHelper output)
        {
            _dbTest = dbTest;
            _output = output;
        }
        [Fact]
        public async Task Handle_ReturnsNull_WhenOrderDoesNotExist()
        {

            await using var context = _dbTest.CreateContext();
            var handler = new Get.Handler(context);
            var nonExistentOrderId = Guid.NewGuid();
            var request = new Get.Request(nonExistentOrderId);

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.Equal(result, []);
        }

        [Fact]
        public async Task Handle_ReturnsOrders_WhenUserExists()
        {
            // Arrange
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


            var request = new Get.Request(userId);

            var handler = new Get.Handler(context);

            var response = await handler.Handle(request, CancellationToken.None);


            Assert.NotNull(response);
            Assert.Equal(order.Id, response.First().Id);
        }
    }
}