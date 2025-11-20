using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products;
using Bogus;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;
using TarlBreuJacoBaraKnor.Core.Domain.Identity.Entities;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering;

namespace TarlBreuJacoBaraKnor.Infrastructure.Data;

public class DbSeeder
{
    private static UserManager<User>? ShopUserManager { get; set; }
    private static RoleManager<IdentityRole<Guid>>? ShopRoleManager { get; set; }
    private static ILogger<DbSeeder>? Logger { get; set; }
    private static ShopContext? Context { get; set; }
    public static async Task SeedData(IApplicationBuilder app)
    {
        // Create a scoped service provider to resolve dependencies
        using var scope = app.ApplicationServices.CreateScope();

        // resolve the logger service
        Logger = scope.ServiceProvider.GetService<ILogger<DbSeeder>>();

        try
        {
            // resolve other dependencies
            ShopUserManager = scope.ServiceProvider.GetService<UserManager<User>>();
            ShopRoleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole<Guid>>>() ?? throw new NotImplementedException("The role manager is missing!");
            Context = scope.ServiceProvider.GetService<ShopContext>();

            await CreateRoles();
            await CreateDefaultAdminUser();
            await GenerateDummyUsers(30);
            await GenerateFoodItems(30);
            await GenerateOrders(50);

        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex.Message);
        }
    }

    private static async Task GenerateFoodItems(int count)
    {
        // Seed food items if none exist
        if (!await Context.FoodItems.AnyAsync())
        {    
            var faker = new Faker();
            var foodItems = new List<FoodItem>();

            for (int i = 0; i < count; i++)
            {
                var item = new FoodItem(
                    name: faker.Lorem.Sentence(3, 2),
                    description: faker.Lorem.Sentence(10, 10)
                )
                {
                    Price = faker.Random.Decimal(50, 250) + 0.99m,
                    CookTime = faker.Random.Number(5, 20)
                };

                foodItems.Add(item);
            }

            await Context.FoodItems.AddRangeAsync(foodItems);
            await Context.SaveChangesAsync();
            Logger.LogInformation($"Seeded {foodItems.Count()} food items");
        }
    }

    private static async Task GenerateOrders(int count)
    {
        // Only seed if no orders exist
        if (await Context.Orders.AnyAsync())
        {
            Logger.LogInformation("Orders already exist, skipping seeding");
            return;
        }

        var faker = new Faker();
        var users = await Context.Users.Where(u => u.AccountState == AccountStates.Approved).ToListAsync();
        var foodItems = await Context.FoodItems.ToListAsync();

        Logger.LogInformation($"Found {users.Count} approved users and {foodItems.Count} food items for order seeding");

        if (!users.Any())
        {
            Logger.LogWarning("Cannot seed orders: no approved users found");
            return;
        }
        
        if (!foodItems.Any())
        {
            Logger.LogWarning("Cannot seed orders: no food items found");
            return;
        }

        var orders = new List<Order>();

        for (int i = 0; i < count; i++)
        {
            try
            {
                var user = faker.PickRandom(users);
                var location = new Location
                {
                    Building = faker.PickRandom("Main Building", "Science Hall", "Engineering Building", "Library", "Student Center"),
                    RoomNumber = faker.Random.Number(100, 999).ToString(),
                    Notes = faker.Lorem.Sentence(5)
                };

                // Create order
                var order = new Order(
                    location: location,
                    customer: user,
                    notes: faker.Lorem.Sentence(3)
                )
                {
                    Status = faker.PickRandom<Status>(),
                    // Create orders from the past 30 days
                    OrderDate = DateTimeOffset.UtcNow.AddDays(-faker.Random.Number(0, 30))
                };

                // Add random number of order lines (1-5)
                var itemCount = faker.Random.Number(1, 5);
                
                for (int j = 0; j < itemCount; j++)
                {
                    var foodItem = faker.PickRandom(foodItems);
                    var quantity = faker.Random.Number(1, 3);
                    
                    // Create a deterministic Guid from the food item ID
                    var foodItemGuid = new Guid($"00000000-0000-0000-0000-{foodItem.Id:D12}");
                    
                    var orderLine = new OrderLine(
                        foodItemId: foodItemGuid,
                        foodItemName: foodItem.Name,
                        amount: quantity,
                        price: foodItem.Price
                    );

                    order.AddOrderLine(orderLine);
                }

                orders.Add(order);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to create order {i}: {ex.Message} - {ex.StackTrace}");
            }
        }

        if (orders.Any())
        {
            await Context.Orders.AddRangeAsync(orders);
            await Context.SaveChangesAsync();
            Logger.LogInformation($"Seeded {orders.Count} orders with a total of {orders.Sum(o => o.OrderLines.Count)} order lines");
        }
        else
        {
            Logger.LogWarning("No orders were created");
        }
    }

    private static async Task CreateRoles()
    {
        foreach (Roles role in Enum.GetValues<Roles>())
        {
            if ((await ShopRoleManager.RoleExistsAsync(role.ToString())) == false)
            {
                await ShopRoleManager.CreateAsync(new IdentityRole<Guid>(role.ToString()));
            }
        }
    }

    private static async Task CreateDefaultAdminUser()
    {
        // Check if any users exist to prevent duplicate seeding
        if (await ShopUserManager.FindByEmailAsync("admin@gmail.com") == null)
        {
            var user = new User
            {
                Name = "Admin",
                UserName = "admin",
                Email = "admin@gmail.com",
                Address = "Admin Street",
                City = "Admin City",
                PostalCode = "1234",
                AccountState = AccountStates.Approved,
                ChangePasswordOnFirstLogin = true,
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            
            // Attempt to create admin user
            var createUserResult = await ShopUserManager
                .CreateAsync(user: user, password: "Admin123456789!");


            // Validate user creation
            if (createUserResult.Succeeded == false)
            {
                var errors = createUserResult.Errors.Select(e => e.Description);
                Logger.LogError(
                    $"Failed to create admin user. Errors: {string.Join(", ", errors)}"
                );
                return;
            }

            // adding role to user
            var addUserToRoleResult = await ShopUserManager
                            .AddToRoleAsync(user: user, role: Roles.Admin.ToString());

            if (addUserToRoleResult.Succeeded == false)
            {
                var errors = addUserToRoleResult.Errors.Select(e => e.Description);
                Logger.LogError($"Failed to add admin role to user. Errors : {string.Join(",", errors)}");
            }
            Logger.LogInformation("Admin user is created");
        }
    }

    private static async Task GenerateDummyUsers(int count)
    {
        if (Context.Users.Count() > count)
            return;

        var faker = new Faker();
        var users = new List<User>();

        for (int i = 0; i < count; i++)
        {
            var name = faker.Name.FullName();
            var user = new User
                {
                    Name = name,
                    UserName = name.ToLower().Replace(" ", "") + faker.Random.Int(0, 999).ToString(),
                    Email = faker.Internet.Email(),
                    City = faker.Address.City(),
                    Address = faker.Address.StreetAddress(),
                    PostalCode = faker.Address.CountryCode(),
                    AccountState = faker.Random.Enum<AccountStates>()
                };

            var result = await ShopUserManager.CreateAsync(user, "SuperSecretPassword123/+++");

            if (result.Succeeded)
            {
                var role = faker.Random.Enum<Roles>().ToString();
                await ShopUserManager.AddToRoleAsync(user, role);
            }
            else
            {
                Console.WriteLine($"Failed to create user {name}: " +
                    $"{string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        Logger.LogInformation($"Seeded {users.Count()} food items");

    }
}