using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservant.Api.Models;

namespace Reservant.Api.Data.EntityConfigs;

/// <summary>
/// Entity type config for Report
/// </summary>
public class ReportConfig : IEntityTypeConfiguration<Report>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.HasOne(report => report.ReportedUser).WithMany();
        builder.HasOne(report => report.CreatedBy).WithMany();
        builder.HasOne(report => report.Visit).WithMany();

        builder.OwnsOne(x => x.Resolution);
        
        builder
            .HasOne(report => report.Thread)
            .WithOne()
            .HasForeignKey<Report>(report => report.ThreadId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
