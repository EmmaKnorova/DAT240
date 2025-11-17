using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products;
using Bogus;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.Infrastructure.Data;

public class DbSeeder
{
    public static async Task SeedData(IApplicationBuilder app)
    {
        // Create a scoped service provider to resolve dependencies
        using var scope = app.ApplicationServices.CreateScope();

        // resolve the logger service
        var logger = scope.ServiceProvider.GetService<ILogger<DbSeeder>>();

        try
        {
            // resolve other dependencies
            var userManager = scope.ServiceProvider.GetService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole<Guid>>>();
            var context = scope.ServiceProvider.GetService<ShopContext>();

            // Create roles
            foreach (Roles role in Enum.GetValues<Roles>())
            {
                if ((await roleManager.RoleExistsAsync(role.ToString())) == false)
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role.ToString()));
                }
            }

            // Check if any users exist to prevent duplicate seeding
            if (await userManager.FindByEmailAsync("admin@gmail.com") == null)
            {
                var user = new User
                {
                    Name = "Admin",
                    UserName = "admin",
                    Email = "admin@gmail.com",
                    Address = "Admin Street",
                    City = "Admin City",
                    PostalCode = "1234",
                    ChangePasswordOnFirstLogin = true,
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                
                // Attempt to create admin user
                var createUserResult = await userManager
                    .CreateAsync(user: user, password: "Admin123456789!");


                // Validate user creation
                if (createUserResult.Succeeded == false)
                {
                    var errors = createUserResult.Errors.Select(e => e.Description);
                    logger.LogError(
                        $"Failed to create admin user. Errors: {string.Join(", ", errors)}"
                    );
                    return;
                }

                // adding role to user
                var addUserToRoleResult = await userManager
                                .AddToRoleAsync(user: user, role: Roles.Admin.ToString());

                if (addUserToRoleResult.Succeeded == false)
                {
                    var errors = addUserToRoleResult.Errors.Select(e => e.Description);
                    logger.LogError($"Failed to add admin role to user. Errors : {string.Join(",", errors)}");
                }
                logger.LogInformation("Admin user is created");
            }

            // Seed food items if none exist
            if (!await context.FoodItems.AnyAsync())
            {
                var foodItems = GenerateFoodItems(30);
                await context.FoodItems.AddRangeAsync(foodItems);
                await context.SaveChangesAsync();
                logger.LogInformation($"Seeded {foodItems.Length} food items");
            }

        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
        }
    }

    private static FoodItem[] GenerateFoodItems(int count)
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

        return foodItems.ToArray();
    }
}