using System;
using System.Threading;
using System.Threading.Tasks;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.Core.Domain.Identity.Entities;
using TarlBreuJacoBaraKnor.webapp.Tests.Helpers;
using Xunit;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Core.Domain.Ordering.Pipelines;

public class GetByCustomerNameTests : IClassFixture<DbTest>
{
    private readonly DbTest _dbTest;

    public GetByCustomerNameTests(DbTest dbTest)
    {
        _dbTest = dbTest;
    }

    private User CreateTestUser(string name, string email)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            UserName = email.Split('@')[0],
            Address = "123 Test Street",
            City = "Test City",
            PostalCode = "12345",
            AccountState = AccountStates.Approved
        };
    }

    [Fact]
    public async Task Handle_WhenUserExists_ReturnsUser()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var user = CreateTestUser("John Doe", "john@test.com");
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var handler = new GetByCustomerName.Handler(context);
        var request = new GetByCustomerName.Request("John Doe");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal("john@test.com", result.Email);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var handler = new GetByCustomerName.Handler(context);
        var request = new GetByCustomerName.Request("Nonexistent User");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WhenMultipleUsers_ReturnsCorrectUser()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var user1 = CreateTestUser("Alice Smith", "alice@test.com");
        var user2 = CreateTestUser("Bob Jones", "bob@test.com");
        var user3 = CreateTestUser("Charlie Brown", "charlie@test.com");
        
        await context.Users.AddRangeAsync(user1, user2, user3);
        await context.SaveChangesAsync();

        var handler = new GetByCustomerName.Handler(context);
        var request = new GetByCustomerName.Request("Bob Jones");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Bob Jones", result.Name);
        Assert.Equal("bob@test.com", result.Email);
    }

    [Fact]
    public async Task Handle_NameIsCaseSensitive()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var user = CreateTestUser("John Doe", "john@test.com");
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var handler = new GetByCustomerName.Handler(context);
        var request = new GetByCustomerName.Request("john doe"); // lowercase

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Null(result); // Should not find user due to case mismatch
    }

    [Fact]
    public async Task Handle_ReturnsUserWithAllProperties()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
            UserName = "testuser",
            Address = "456 Main St",
            City = "Springfield",
            PostalCode = "54321",
            AccountState = AccountStates.Pending
        };
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var handler = new GetByCustomerName.Handler(context);
        var request = new GetByCustomerName.Request("Test User");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("Test User", result.Name);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("testuser", result.UserName);
        Assert.Equal("456 Main St", result.Address);
        Assert.Equal("Springfield", result.City);
        Assert.Equal("54321", result.PostalCode);
        Assert.Equal(AccountStates.Pending, result.AccountState);
    }

    [Fact]
    public async Task Handle_WithEmptyString_ReturnsNull()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var user = CreateTestUser("John Doe", "john@test.com");
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var handler = new GetByCustomerName.Handler(context);
        var request = new GetByCustomerName.Request("");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WithWhitespace_ReturnsNull()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var user = CreateTestUser("John Doe", "john@test.com");
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var handler = new GetByCustomerName.Handler(context);
        var request = new GetByCustomerName.Request("   ");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WithNameContainingSpecialCharacters_ReturnsUser()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var user = CreateTestUser("O'Brien-Smith", "obrien@test.com");
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var handler = new GetByCustomerName.Handler(context);
        var request = new GetByCustomerName.Request("O'Brien-Smith");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("O'Brien-Smith", result.Name);
    }

    [Fact]
    public void Handle_ThrowsArgumentNullException_WhenDbContextIsNull()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => new GetByCustomerName.Handler(null!));
    }

    [Fact]
    public async Task Handle_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        using var context = _dbTest.CreateContext();
        var user = CreateTestUser("John Doe", "john@test.com");
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var handler = new GetByCustomerName.Handler(context);
        var request = new GetByCustomerName.Request("John Doe");
        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        var result = await handler.Handle(request, cancellationTokenSource.Token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John Doe", result.Name);
    }
}