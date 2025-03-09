using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Models;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Reservant.Api.Data;

public class ApiDbContext(
    DbContextOptions<ApiDbContext> options,
    IConfiguration configuration,
    UserIdService userIdService
    ) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    private readonly Guid? _userId = userIdService.GetUserId();

    public DbSet<FileUpload> FileUploads { get; init; } = null!;

    public DbSet<Restaurant> Restaurants { get; init; } = null!;

    public DbSet<Employment> Employments { get; init; } = null!;

    public DbSet<RestaurantPhoto> RestaurantPhotos { get; init; } = null!;

    public DbSet<RestaurantTag> RestaurantTags { get; init; } = null!;

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

    public DbSet<Report> Reports { get; init; } = null!;

    static ApiDbContext()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    /// <summary>
    /// Drop all tables in the database
    /// </summary>
    public async Task DropAllTablesAsync()
    {
        await Database.ExecuteSqlRawAsync(
            """
            DO $$
            DECLARE
                sql TEXT;
            BEGIN
                SELECT STRING_AGG(command, E'\n')
                INTO sql
                FROM (
                    -- Drop all foreign key constraints
                    SELECT FORMAT('ALTER TABLE %I DROP CONSTRAINT %I;', tab.relname, con.conname) AS command
                    FROM pg_catalog.pg_constraint con
                    JOIN pg_catalog.pg_class tab ON con.conrelid = tab.oid
                    WHERE con.contype = 'f'
                      AND tab.relname NOT LIKE 'pg_%'
                      AND tab.relname NOT LIKE 'sql_%'
                      AND tab.relname != 'spatial_ref_sys'
                    UNION ALL
                    -- Drop every table
                    SELECT FORMAT('DROP TABLE %I;', tab.relname) AS command
                    FROM pg_catalog.pg_class tab
                    WHERE tab.relkind = 'r'
                      AND tab.relname NOT LIKE 'pg_%'
                      AND tab.relname NOT LIKE 'sql_%'
                      AND tab.relname != 'spatial_ref_sys'
                ) commands;

                IF sql IS NOT NULL THEN
                    EXECUTE sql;
                END IF;
            END
            $$;
            """);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connString = configuration.GetConnectionString("Default");

        optionsBuilder.UseNpgsql(
            connString ?? throw new InvalidOperationException("Connection string 'Default' not found"),
            x => x.UseNetTopologySuite());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp without time zone");
        configurationBuilder.Properties<DateTime?>().HaveColumnType("timestamp without time zone");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApiDbContext).Assembly);

        builder.Entity<Notification>()
            .HasQueryFilter(n => n.TargetUserId == _userId);


        var softDeletableEntities =
            from entity in builder.Model.GetEntityTypes()
            where entity.ClrType.IsAssignableTo(typeof(ISoftDeletable))
            select entity;

        foreach (var entity in softDeletableEntities)
        {
            SetSoftDeletableQueryFilterMethod
                .MakeGenericMethod(entity.ClrType)
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
}
