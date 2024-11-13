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
    }
}
