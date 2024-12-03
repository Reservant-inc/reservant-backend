using AutoMapper;
using Reservant.Api.Dtos.Users;
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
            // Mapowanie User → UserSummaryVM
            CreateMap<User, UserSummaryVM>();

            // Mapowanie Report → ReportVM
            CreateMap<Report, ReportVM>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.ResolvedBy, opt => opt.MapFrom(src =>
                    src.Resolution != null && src.Resolution.ResolvedBy != null
                        ? $"{src.Resolution.ResolvedBy.FirstName} {src.Resolution.ResolvedBy.LastName}"
                        : null))
                .ForMember(dest => dest.SupportComment, opt => opt.MapFrom(src =>
                    src.Resolution != null ? src.Resolution.SupportComment : null))
                .ForMember(dest => dest.ResolutionDate, opt => opt.MapFrom(src =>
                    src.Resolution != null ? src.Resolution.Date : (DateTime?)null));
        }
    }
}