using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservant.Api.Models;

namespace Reservant.Api.Data.EntityConfigs;

/// <summary>
/// Entity type config for ParticipationRequest
/// </summary>
public class ParticipationRequestConfig : IEntityTypeConfiguration<ParticipationRequest>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ParticipationRequest> builder)
    {
        builder.HasQueryFilter(pr => pr.DateDeleted == null);
    }
}
