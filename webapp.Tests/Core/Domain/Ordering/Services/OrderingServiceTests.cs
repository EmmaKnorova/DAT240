using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.DTOs;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace webapp.Tests.Core.Domain.Ordering.Services
{
    public class OrderingServiceTests : IClassFixture<DbTest>
    {
        private readonly DbTest _dbTest;
        private readonly ITestOutputHelper _output;

        public OrderingServiceTests(DbTest dbTest, ITestOutputHelper output)
        {
            _dbTest = dbTest;
            _output = output;
        }
        [Fact]
        public async Task PlaceOrder_SendsAddOrderAndSaveChanges()
        {

            await using var context = _dbTest.CreateContext();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = "test@example.com",
                UserName = "test@example.com",
                Address = "123 Test St",
                City = "Test City",
                PostalCode = "12345"
            };

            var location = new Location { Building = "1", RoomNumber = "2", Notes = " " };

            var orderLines = new[]
            {
                new OrderLineDto(1, "Pizza", 2, 10.0m),
                new OrderLineDto(2, "Burger", 1, 7.5m)
            };

            var mediatorMock = new Mock<IMediator>();

            mediatorMock
                .Setup(m => m.Send(It.IsAny<AddOrder.Request>(), It.IsAny<CancellationToken>()))
                .Returns<AddOrder.Request, CancellationToken>((req, ct) =>
                {
                    var handler = new AddOrder.Handler(context);
                    return handler.Handle(req, ct); 
                });

            mediatorMock
                .Setup(m => m.Send(It.IsAny<SaveChanges.Request>(), It.IsAny<CancellationToken>()))
                .Returns<SaveChanges.Request, CancellationToken>(async (req, ct) =>
                {
                    await context.SaveChangesAsync(ct);
                    return Unit.Value;
                });

            var service = new OrderingService(mediatorMock.Object);

            // Act
            var orderId = await service.PlaceOrder(location, user, orderLines, "Extra notes", 50m,"pi_test_123");

            // Assert
            Assert.NotEqual(Guid.Empty, orderId);

            var savedOrder = await context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Location)
                .Include(o => o.OrderLines)
                .SingleOrDefaultAsync(o => o.Id == orderId);

            Assert.NotNull(savedOrder);
            Assert.Equal(user.Id, savedOrder.Customer.Id);
            Assert.Equal("1", savedOrder.Location.Building);
            Assert.Equal(2, savedOrder.OrderLines.Count); // Two order OrderLines
            Assert.Contains(savedOrder.OrderLines, l => l.FoodItemName == "Pizza");
            Assert.Contains(savedOrder.OrderLines, l => l.FoodItemName == "Burger");
                
        }


    }
}