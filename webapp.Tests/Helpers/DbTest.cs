using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MediatR;
using Moq;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Tests.Helpers;

public class DbTest : IDisposable
{
    private readonly IMediator _mediator;

    public DbTest()
    {
        // Mock mediator for tests
        var mediatorMock = new Mock<IMediator>();
        _mediator = mediatorMock.Object;
    }

    // Create a new isolated context for each test
    public ShopContext CreateContext()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var contextOptions = new DbContextOptionsBuilder<ShopContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ShopContext(contextOptions, _mediator);
        
        // Create schema for this database
        context.Database.EnsureCreated();
        
        return context;
    }

    public void Dispose()
    {
        // Cleanup handled by disposing contexts in tests
    }
}