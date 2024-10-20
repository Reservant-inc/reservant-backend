using AutoMapper;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Tables;

/// <summary>
/// Mapping profile for table DTOs
/// </summary>
public class Mappings : Profile
{
    /// <inheritdoc />
    public Mappings()
    {
        CreateMap<Table, TableVM>();
    }
}
