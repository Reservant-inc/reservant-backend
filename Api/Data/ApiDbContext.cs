using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Models;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Reservant.Api.Data;

public class ApiDbContext(DbContextOptions<ApiDbContext> options, IConfiguration configuration) : IdentityDbContext<User>(options)
{
    public required DbSet<WeatherForecast> WeatherForecasts { get; init; }

    public DbSet<FileUpload> FileUploads { get; init; } = null!;

    public DbSet<Restaurant> Restaurants { get; init; } = null!;

    public DbSet<Employment> Employments { get; init; } = null!;

    public DbSet<RestaurantPhoto> RestaurantPhotos { get; init; } = null!;

    public DbSet<RestaurantTag> RestaurantTags { get; init; } = null!;

    public DbSet<Table> Tables { get; init; } = null!;

    public DbSet<RestaurantGroup> RestaurantGroups { get; init; } = null!;

    public DbSet<Menu> Menus { get; init; } = null!;

    public DbSet<MenuItem> MenuItems { get; init; } = null!;

    public DbSet<Visit> Visits { get; init; } = null!;

    public DbSet<Order> Orders { get; init; } = null!;

    public DbSet<OrderItem> OrderItems { get; init; } = null!;

    public DbSet<FriendRequest> FriendRequests { get; init; } = null!;

    public DbSet<Event> Events { get; init; } = null!;

    public DbSet<Review> Reviews { get; init; } = null!;

    public DbSet<MessageThread> MessageThreads { get; init; } = null!;

    public DbSet<Message> Messages { get; init; } = null!;

    public DbSet<PaymentTransaction> PaymentTransactions { get; init; } = null!;

    public DbSet<Ingredient> Ingredients { get; init; } = null!;

    public DbSet<Delivery> Deliveries { get; init; } = null!;

    public DbSet<IngredientMenuItem> IngredientMenuItems { get; set; } = null!;

    public DbSet<Notification> Notifications { get; set; } = null!;

    /// <summary>
    /// Drop all tables in the database
    /// </summary>
    public async Task DropAllTablesAsync()
    {
        await Database.ExecuteSqlRawAsync("EXEC DropAllTables");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connString = configuration.GetConnectionString("Default");

        optionsBuilder.UseSqlServer(
            connString ?? throw new InvalidOperationException("Connection string 'Default' not found"),
            x => x.UseNetTopologySuite());
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Restaurant>()
            .Property(r => r.Location)
            .HasColumnType("geography");

        builder.Entity<Visit>(eb =>
        {
            eb.HasOne<Restaurant>(v => v.Restaurant)
                .WithMany()
                .HasForeignKey(v => v.TableRestaurantId);
        });

        builder.Entity<Table>().HasKey(t => new { t.RestaurantId, t.Id });

        builder.Entity<RestaurantPhoto>().HasKey(rp => new { rp.RestaurantId, rp.Order });

        builder.Entity<RestaurantTag>().HasData([
            new RestaurantTag { Name = "OnSite" },
            new RestaurantTag { Name = "Takeaway" },
            new RestaurantTag { Name = "Asian" },
            new RestaurantTag { Name = "Italian" },
            new RestaurantTag { Name = "Tag1" },
            new RestaurantTag { Name = "Tag2" }
        ]);

        builder.Entity<OrderItem>(eb =>
        {
            eb.HasKey(oi => new { oi.MenuItemId, oi.OrderId });
        });

        builder.Entity<IngredientDelivery>(eb =>
        {
            eb.HasKey(id => new { id.DeliveryId, id.IngredientId });
        });

        builder.Entity<User>(eb =>
        {
            eb.Property(u => u.Id)
                .HasMaxLength(36);

            eb.HasOne<FileUpload>(u => u.Photo)
                .WithOne()
                .HasForeignKey<User>(u => u.PhotoFileName);

            eb.HasMany<Event>(u => u.EventsCreated)
                .WithOne(e => e.Creator)
                .HasForeignKey(e => e.CreatorId);

            eb.HasMany<Event>(u => u.InterestedIn)
                .WithMany(e => e.Interested);

            eb.HasMany<FileUpload>(u => u.Uploads)
                .WithOne(fu => fu.User);

            eb.HasMany<FriendRequest>(u => u.OutgoingRequests)
                .WithOne(fr => fr.Sender);

            eb.HasMany<FriendRequest>(u => u.IncomingRequests)
                .WithOne(fr => fr.Receiver);

            eb.HasMany(u => u.Threads)
                .WithMany(mt => mt.Participants);
        });

        builder.Entity<MessageThread>(eb =>
        {
            eb.HasOne(mt => mt.Creator).WithMany();
        });

        builder.Entity<IngredientMenuItem>()
                .HasKey(im => new { im.MenuItemId, im.IngredientId });

        builder.Entity<FriendRequest>()
            .HasQueryFilter(fr => fr.DateDeleted == null);

        builder.Entity<Notification>(eb =>
        {
            eb.Property(n => n.Details).HasConversion(
                value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                column => JsonSerializer.Deserialize<JsonElement>(column, (JsonSerializerOptions?)null));
        });

        var softDeletableEntities =
            from prop in GetType().GetProperties()
            where prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
            let entity = prop.PropertyType.GenericTypeArguments[0]
            where entity.IsAssignableTo(typeof(ISoftDeletable))
            select entity;
        foreach (var entity in softDeletableEntities)
        {
            SetSoftDeletableQueryFilterMethod
                .MakeGenericMethod(entity)
                .Invoke(null, [builder]);
        }

        // Microsoft suggest we do not configure cascade delete when using soft delete
        var relationships = builder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys());
        foreach (var relationship in relationships)
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    private static readonly MethodInfo SetSoftDeletableQueryFilterMethod =
        typeof(ApiDbContext).GetMethod(
            nameof(SetSoftDeletableQueryFilter), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static void SetSoftDeletableQueryFilter<TEntity>(ModelBuilder builder)
        where TEntity : class, ISoftDeletable
    {
        builder.Entity<TEntity>()
            .HasQueryFilter(e => !e.IsDeleted);
    }

    public override int SaveChanges()
    {
        SetIsDeletedOnDeletedEntries();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetIsDeletedOnDeletedEntries();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        SetIsDeletedOnDeletedEntries();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new())
    {
        SetIsDeletedOnDeletedEntries();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void SetIsDeletedOnDeletedEntries()
    {
        ChangeTracker.DetectChanges();
        var deleted = ChangeTracker.Entries()
            .Where(e => e is { State: EntityState.Deleted, Entity: ISoftDeletable });

        foreach (var entry in deleted)
        {
            var entity = (ISoftDeletable)entry.Entity;
            entity.IsDeleted = true;
            entry.State = EntityState.Modified;
        }
    }
}
