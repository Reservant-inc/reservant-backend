using AutoMapper;
using Reservant.Api.Dtos.Users;
using Reservant.Api.Mapping;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Reports;

/// <summary>
/// Mapping profiles for report DTOs
/// </summary>
public class Mappings : Profile
{
    /// <inheritdoc />
    public Mappings()
    {
        CreateMap<Report, ReportVM>()
            .MapMemberFrom(dest => dest.CreatedBy, src => src.CreatedBy)
            .MapMemberFrom(dest => dest.ResolvedBy, src =>
                src.Resolution != null ? src.Resolution.ResolvedBy : null)
            .MapMemberFrom(dest => dest.ResolutionComment, src =>
                src.Resolution != null ? src.Resolution.SupportComment : null)
            .MapMemberFrom(dest => dest.ResolutionDate, src =>
                src.Resolution != null ? src.Resolution.Date : (DateTime?)null);

        CreateMap<ReportAssignment, AssignedAgentVM>();
    }
}
