using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Models;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Reservant.Api.Data;

public class ApiDbContext(DbContextOptions<ApiDbContext> options) : IdentityDbContext<User>(options)
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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

        builder.Entity<Restaurant>().HasQueryFilter(r => !r.IsDeleted);

        builder.Entity<RestaurantGroup>().HasQueryFilter(r => !r.IsDeleted);

        builder.Entity<Employment>().HasKey(e => new { e.EmployeeId, e.RestaurantId });
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
