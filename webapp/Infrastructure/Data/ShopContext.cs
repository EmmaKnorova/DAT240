using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Cart;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products;
using TarlBreuJacoBaraKnor.webapp.SharedKernel;

namespace TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

public class ShopContext : DbContext
{
    private readonly IMediator? _mediator;

    public ShopContext(DbContextOptions<ShopContext> options, IMediator? mediator = null)
        : base(options)
    {
        _mediator = mediator;
    }

    public DbSet<FoodItem> FoodItems => Set<FoodItem>();
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // -------- FoodItem --------
        b.Entity<FoodItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(120);
            e.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(2048);
            e.Property(x => x.Price)
                .HasPrecision(10, 2);
            e.Property(x => x.CookTime);
        });

        // -------- ShoppingCart / CartItem --------
        b.Entity<ShoppingCart>(e =>
{
			e.HasKey(x => x.Id);
			e.HasMany(x => x.Items)
			.WithOne()
			.HasForeignKey("cart_id")
			.OnDelete(DeleteBehavior.Cascade);

			var nav = e.Metadata.FindNavigation(nameof(ShoppingCart.Items))!;
			nav.SetField("_items");
			nav.SetPropertyAccessMode(PropertyAccessMode.Field);
		});

        b.Entity<CartItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Sku).IsRequired();
            e.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);
            e.Property(x => x.Price)
                .HasPrecision(10, 2);
            e.Property(x => x.Count)
                .IsRequired();
        });

        // -------- Users + Roles (primitive collection) --------
        b.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(120);
            e.Property(x => x.Email).IsRequired().HasMaxLength(256);
            e.Property(x => x.PhoneNumber).HasMaxLength(32);
            e.Property(x => x.Password).IsRequired().HasMaxLength(256);

            // EF Core primitive collection ->  user_roles table (user_id, role)
            // e.PrimitiveCollection(x => x.Roles)
            //  .ToTable("user_roles")
            //  .WithForeignKey("user_id")
            //  .Element(e =>
            //  {
            //      e.Property().HasConversion<string>().HasMaxLength(32).HasColumnName("role");
            //  });

            e.HasIndex(x => x.Email).IsUnique();
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (_mediator is null) return result;

        // Domain Events dispatch
        var entitiesWithEvents = ChangeTracker.Entries<BaseEntity>()
            .Select(e => e.Entity)
            .Where(e => e.Events.Any())
            .ToArray();

        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.Events.ToArray();
            entity.Events.Clear();
            foreach (var domainEvent in events)
                await _mediator.Publish(domainEvent, cancellationToken);
        }

        return result;
    }

    public override int SaveChanges() => SaveChangesAsync().GetAwaiter().GetResult();
}