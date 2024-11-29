using AutoMapper;
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
        CreateMap<Report, ReportVM>();
        
        // Mapowanie dla ReportResolutionVM
        CreateMap<Report, ReportResolutionVM>()
            .ForMember(dest => dest.ResolvedBy, opt => opt.MapFrom(src => 
                src.Resolution != null 
                    ? $"{src.Resolution.ResolvedBy.FirstName} {src.Resolution.ResolvedBy.LastName}" 
                    : string.Empty))
            .ForMember(dest => dest.ResolvedDate, opt => opt.MapFrom(src => 
                src.Resolution != null 
                    ? src.Resolution.Date 
                    : (DateTime?)null))
            .ForMember(dest => dest.SupportComment, opt => opt.MapFrom(src => 
                src.Resolution != null 
                    ? src.Resolution.SupportComment 
                    : string.Empty))
            .ForMember(dest => dest.ReportDescription, opt => opt.MapFrom(src => src.Description));
    }
}
