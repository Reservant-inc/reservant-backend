using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservant.Api.Models;

namespace Reservant.Api.Data.EntityConfigs;

/// <summary>
/// Entity type config for ReportAssignment
/// </summary>
public class ReportAssignmentConfig : IEntityTypeConfiguration<ReportAssignment>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ReportAssignment> builder)
    {
        builder.HasKey(ra => new { ra.ReportId, ra.AgentId, ra.From });
        builder.HasOne(ra => ra.Agent).WithMany();
        builder.HasOne(ra => ra.Report)
            .WithMany(r => r.AssignedAgents);
    }
}
