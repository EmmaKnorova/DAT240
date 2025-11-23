using System.Threading;
using System.Threading.Tasks;
using Moq;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Pipelines;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Services;
using Xunit;

namespace webapp.Tests.Core.Domain.Users.Pipelines
{
    public class ApproveTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Approve.Handler _handler;

        public ApproveTests()
        {
            _mockUserService = new Mock<IUserService>();
            _handler = new Approve.Handler(_mockUserService.Object);
        }

        [Fact]
        public async Task Handle_CallsApproveUserState_WithCorrectUserId()
        {
            const string testUserId = "user-12345";
            var request = new Approve.Request(testUserId);
            var cancellationToken = CancellationToken.None;

            _mockUserService.Setup(
                s => s.ApproveUserState(testUserId, cancellationToken)
            ).Returns(Task.CompletedTask);

            await _handler.Handle(request, cancellationToken);

            _mockUserService.Verify(
                s => s.ApproveUserState(
                    It.Is<string>(id => id == testUserId), 
                    It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task Handle_CompletesSuccessfully_WhenUserDoesNotExist()
        {
            const string nonExistentUserId = "non-existent-id";
            var request = new Approve.Request(nonExistentUserId);

            _mockUserService.Setup(
                s => s.ApproveUserState(nonExistentUserId, It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            var exception = await Record.ExceptionAsync(() => _handler.Handle(request, CancellationToken.None));
            
            Assert.Null(exception);

            _mockUserService.Verify(
                s => s.ApproveUserState(nonExistentUserId, It.IsAny<CancellationToken>()), 
                Times.Once);
        }
    }
}