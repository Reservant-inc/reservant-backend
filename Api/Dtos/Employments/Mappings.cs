using AutoMapper;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Employments;

/// <summary>
/// Mapping profile for employment DTOs
/// </summary>
public class Mappings : Profile
{
    /// <inheritdoc />
    public Mappings()
    {
        CreateMap<Employment, EmploymentVM>();
    }
}
