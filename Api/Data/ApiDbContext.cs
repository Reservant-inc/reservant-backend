using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Models;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Reservant.Api.Data;

public class ApiDbContext(
    DbContextOptions<ApiDbContext> options,
    IConfiguration configuration,
    UserIdService userIdService
    ) : IdentityDbContext<User>(options)
{
    private readonly string? _userId = userIdService.GetUserId();

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

    public DbSet<Notification> Notifications { get; set; } = null!;

    public DbSet<ParticipationRequest> EventParticipationRequests { get; init; } = null!;

    /// <summary>
    /// Drop all tables in the database
    /// </summary>
    public async Task DropAllTablesAsync()
    {
        await Database.ExecuteSqlRawAsync(
            """
            -- Drop all foreign key constraints
        
            DECLARE @sql NVARCHAR(MAX) = N'';
        
            SELECT @sql += N'
            ALTER TABLE ' + QUOTENAME(TABLE_NAME) + ' DROP CONSTRAINT ' + QUOTENAME(CONSTRAINT_NAME) + ';'
            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
            WHERE CONSTRAINT_TYPE = 'FOREIGN KEY';
        
            EXEC sp_sqlexec @sql;
        
            -- Drop every table
        
            EXEC sp_msforeachtable 'DROP TABLE ?';
            """);
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

        builder.ApplyConfigurationsFromAssembly(typeof(ApiDbContext).Assembly);

        builder.Entity<Notification>()
            .HasQueryFilter(n => n.TargetUserId == _userId);

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
