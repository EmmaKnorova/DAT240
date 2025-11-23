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
    public class RegisterInternalUserTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly RegisterInternalUser.Handler _handler;
        private readonly RegisterInputModel _mockRegistrationDto;

        public RegisterInternalUserTests()
        {
            _mockUserService = new Mock<IUserService>();
            _handler = new RegisterInternalUser.Handler(_mockUserService.Object);
            _mockRegistrationDto = new RegisterInputModel 
            { 
                Email = "new.user@example.com", 
                Password = "SecurePassword123",
                Name = "New User",
                Role = "Customer"
            };
        }

        [Fact]
        public async Task Handle_CallsRegisterInternalUser_WithCorrectDto()
        {
            var request = new RegisterInternalUser.Request(_mockRegistrationDto);
            var expectedResult = Result.Success();

            _mockUserService.Setup(
                s => s.RegisterInternalUser(_mockRegistrationDto)
            ).ReturnsAsync(expectedResult);

            var result = await _handler.Handle(request, CancellationToken.None);

            _mockUserService.Verify(
                s => s.RegisterInternalUser(
                    It.Is<RegisterInputModel>(dto => 
                        dto.Email == _mockRegistrationDto.Email && dto.Role == _mockRegistrationDto.Role)
                ), 
                Times.Once);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenServiceReturnsFailure()
        {
            var request = new RegisterInternalUser.Request(_mockRegistrationDto);
            var expectedResult = Result.Failure("Email already exists.");

            _mockUserService.Setup(
                s => s.RegisterInternalUser(_mockRegistrationDto)
            ).ReturnsAsync(expectedResult);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Contains("Email already exists.", result.Errors);
        }

        [Fact]
        public async Task Handle_PassesCancellationTokenToService()
        {
            var request = new RegisterInternalUser.Request(_mockRegistrationDto);
            var expectedResult = Result.Success();

            _mockUserService.Setup(
                s => s.RegisterInternalUser(It.IsAny<RegisterInputModel>())
            ).ReturnsAsync(expectedResult);

            await _handler.Handle(request, CancellationToken.None);

            _mockUserService.Verify(
                s => s.RegisterInternalUser(It.IsAny<RegisterInputModel>()),
                Times.Once);
        }
    }
}