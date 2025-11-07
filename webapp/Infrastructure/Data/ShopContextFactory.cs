using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace UiS.Dat240.Lab3.Infrastructure.Data;

public class ShopContextFactory : IDesignTimeDbContextFactory<ShopContext>
{
    public ShopContext CreateDbContext(string[] args)
    {
        // Load appsettings.json and environment variables (including ConnectionStrings__Default)
        var cfg = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cs = cfg.GetConnectionString("Default")
                 ?? "Host=localhost;Port=5432;Database=campuseats;Username=campus;Password=campus_pw";

        var options = new DbContextOptionsBuilder<ShopContext>()
            .UseNpgsql(cs)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new ShopContext(options, mediator: null);
    }
}
