using System.Threading;
using System.Threading.Tasks;
using Moq;
using TarlBreuJacoBaraKnor.Core.Domain.Users.DTOs;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Pipelines;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Services;
using TarlBreuJacoBaraKnor.SharedKernel;
using Xunit;

namespace webapp.Tests.Core.Domain.Users.Pipelines
{
    public class LogInInternalUserTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly LogInInternalUser.Handler _handler;
        private readonly LoginInputModel _mockLoginDto;

        public LogInInternalUserTests()
        {
            _mockUserService = new Mock<IUserService>();
            _handler = new LogInInternalUser.Handler(_mockUserService.Object);
            _mockLoginDto = new LoginInputModel { Email = "test@example.com", Password = "Password123" };
        }

        [Fact]
        public async Task Handle_CallsLogInInternalUser_WithCorrectDto()
        {
            var request = new LogInInternalUser.Request(_mockLoginDto);
            var expectedResult = Result<string>.Success("/Customer/Menu");

            _mockUserService.Setup(
                s => s.LogInInternalUser(_mockLoginDto)
            ).ReturnsAsync(expectedResult);

            var result = await _handler.Handle(request, CancellationToken.None);

            _mockUserService.Verify(
                s => s.LogInInternalUser(
                    It.Is<LoginInputModel>(dto => 
                        dto.Email == _mockLoginDto.Email && dto.Password == _mockLoginDto.Password)
                ), 
                Times.Once);
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedResult.Value, result.Value);
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenServiceReturnsFailure()
        {
            var request = new LogInInternalUser.Request(_mockLoginDto);
            var expectedResult = Result<string>.Failure("Invalid credentials.");

            _mockUserService.Setup(
                s => s.LogInInternalUser(_mockLoginDto)
            ).ReturnsAsync(expectedResult);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(expectedResult.Errors.Count, result.Errors.Count);
            Assert.Contains("Invalid credentials.", result.Errors);
        }

        [Fact]
        public async Task Handle_PassesCancellationTokenToService()
        {
            var request = new LogInInternalUser.Request(_mockLoginDto);
            var expectedResult = Result<string>.Success("/");

            _mockUserService.Setup(
                s => s.LogInInternalUser(It.IsAny<LoginInputModel>())
            ).ReturnsAsync(expectedResult);

            await _handler.Handle(request, CancellationToken.None);

            _mockUserService.Verify(
                s => s.LogInInternalUser(It.IsAny<LoginInputModel>()),
                Times.Once);
        }
    }
}