using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Tests.Helpers;
using Xunit;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Users.Pipelines;

public class InviteToAdminTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;

    public InviteToAdminTests(DbTest dbTest)
    {
        _dbTest = dbTest;
    }

    [Fact]
    public async Task Handle_ValidCustomerUser_PromotesToAdmin()
    {
        // Arrange
        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            UserName = "customer@test.com",
            Email = "customer@test.com",
            Name = "Test Customer"
        };

        userManagerMock
            .Setup(um => um.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        userManagerMock
            .Setup(um => um.IsInRoleAsync(user, Roles.Admin.ToString()))
            .ReturnsAsync(false);

        userManagerMock
            .Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(new[] { Roles.Customer.ToString() });

        userManagerMock
            .Setup(um => um.RemoveFromRolesAsync(user, It.IsAny<string[]>()))
            .ReturnsAsync(IdentityResult.Success);

        userManagerMock
            .Setup(um => um.AddToRoleAsync(user, Roles.Admin.ToString()))
            .ReturnsAsync(IdentityResult.Success);

        var handler = new InviteToAdmin.Handler(userManagerMock.Object);
        var request = new InviteToAdmin.Request(userId);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(response.Success);
        Assert.Empty(response.Errors);
        
        userManagerMock.Verify(um => um.RemoveFromRolesAsync(user, It.Is<string[]>(roles => roles.Contains(Roles.Customer.ToString()))), Times.Once);
        userManagerMock.Verify(um => um.AddToRoleAsync(user, Roles.Admin.ToString()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCourierUser_PromotesToAdmin()
    {
        // Arrange
        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            UserName = "courier@test.com",
            Email = "courier@test.com",
            Name = "Test Courier"
        };

        userManagerMock
            .Setup(um => um.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        userManagerMock
            .Setup(um => um.IsInRoleAsync(user, Roles.Admin.ToString()))
            .ReturnsAsync(false);

        userManagerMock
            .Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(new[] { Roles.Courier.ToString() });

        userManagerMock
            .Setup(um => um.RemoveFromRolesAsync(user, It.IsAny<string[]>()))
            .ReturnsAsync(IdentityResult.Success);

        userManagerMock
            .Setup(um => um.AddToRoleAsync(user, Roles.Admin.ToString()))
            .ReturnsAsync(IdentityResult.Success);

        var handler = new InviteToAdmin.Handler(userManagerMock.Object);
        var request = new InviteToAdmin.Request(userId);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(response.Success);
        Assert.Empty(response.Errors);
        
        userManagerMock.Verify(um => um.RemoveFromRolesAsync(user, It.Is<string[]>(roles => roles.Contains(Roles.Courier.ToString()))), Times.Once);
        userManagerMock.Verify(um => um.AddToRoleAsync(user, Roles.Admin.ToString()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsError()
    {
        // Arrange
        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        var userId = Guid.NewGuid();

        userManagerMock
            .Setup(um => um.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((User?)null);

        var handler = new InviteToAdmin.Handler(userManagerMock.Object);
        var request = new InviteToAdmin.Request(userId);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.Success);
        Assert.Single(response.Errors);
        Assert.Equal("User not found", response.Errors[0]);
        
        userManagerMock.Verify(um => um.IsInRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        userManagerMock.Verify(um => um.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserAlreadyAdmin_ReturnsError()
    {
        // Arrange
        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            UserName = "admin@test.com",
            Email = "admin@test.com",
            Name = "Existing Admin"
        };

        userManagerMock
            .Setup(um => um.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        userManagerMock
            .Setup(um => um.IsInRoleAsync(user, Roles.Admin.ToString()))
            .ReturnsAsync(true);

        var handler = new InviteToAdmin.Handler(userManagerMock.Object);
        var request = new InviteToAdmin.Request(userId);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.Success);
        Assert.Single(response.Errors);
        Assert.Equal("User is already an administrator", response.Errors[0]);
        
        userManagerMock.Verify(um => um.RemoveFromRolesAsync(It.IsAny<User>(), It.IsAny<string[]>()), Times.Never);
        userManagerMock.Verify(um => um.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RemoveRolesFails_ReturnsErrors()
    {
        // Arrange
        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            UserName = "customer@test.com",
            Email = "customer@test.com",
            Name = "Test Customer"
        };

        var identityErrors = new[]
        {
            new IdentityError { Description = "Failed to remove role" }
        };

        userManagerMock
            .Setup(um => um.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        userManagerMock
            .Setup(um => um.IsInRoleAsync(user, Roles.Admin.ToString()))
            .ReturnsAsync(false);

        userManagerMock
            .Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(new[] { Roles.Customer.ToString() });

        userManagerMock
            .Setup(um => um.RemoveFromRolesAsync(user, It.IsAny<string[]>()))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        var handler = new InviteToAdmin.Handler(userManagerMock.Object);
        var request = new InviteToAdmin.Request(userId);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.Success);
        Assert.Single(response.Errors);
        Assert.Equal("Failed to remove role", response.Errors[0]);
        
        userManagerMock.Verify(um => um.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AddAdminRoleFails_ReturnsErrors()
    {
        // Arrange
        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            UserName = "customer@test.com",
            Email = "customer@test.com",
            Name = "Test Customer"
        };

        var identityErrors = new[]
        {
            new IdentityError { Description = "Failed to add admin role" }
        };

        userManagerMock
            .Setup(um => um.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        userManagerMock
            .Setup(um => um.IsInRoleAsync(user, Roles.Admin.ToString()))
            .ReturnsAsync(false);

        userManagerMock
            .Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(new[] { Roles.Customer.ToString() });

        userManagerMock
            .Setup(um => um.RemoveFromRolesAsync(user, It.IsAny<string[]>()))
            .ReturnsAsync(IdentityResult.Success);

        userManagerMock
            .Setup(um => um.AddToRoleAsync(user, Roles.Admin.ToString()))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        var handler = new InviteToAdmin.Handler(userManagerMock.Object);
        var request = new InviteToAdmin.Request(userId);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(response.Success);
        Assert.Single(response.Errors);
        Assert.Equal("Failed to add admin role", response.Errors[0]);
    }

    [Fact]
    public void Constructor_NullUserManager_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new InviteToAdmin.Handler(null!));
    }
}