using AutoMapper;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Reports
{
    /// <summary>
    /// Mapping profiles for report DTOs
    /// </summary>
    public class Mappings : Profile
    {
        /// <inheritdoc />
        public Mappings()
        {
            CreateMap<Report, ReportVM>()
                // Mapowanie pola CreatedBy
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src =>
                    $"{src.CreatedBy.FirstName} {src.CreatedBy.LastName}"))
                
                // Mapowanie pola ResolvedBy
                .ForMember(dest => dest.ResolvedBy, opt => opt.MapFrom(src =>
                    src.Resolution != null && src.Resolution.ResolvedBy != null
                        ? $"{src.Resolution.ResolvedBy.FirstName} {src.Resolution.ResolvedBy.LastName}"
                        : null))
                
                // Mapowanie pola SupportComment
                .ForMember(dest => dest.SupportComment, opt => opt.MapFrom(src =>
                    src.Resolution != null
                        ? src.Resolution.SupportComment
                        : null))
                
                // Mapowanie pola ResolutionDate
                .ForMember(dest => dest.ResolutionDate, opt => opt.MapFrom(src =>
                    src.Resolution != null
                        ? src.Resolution.Date
                        : (DateTime?)null));
        }
    }
}