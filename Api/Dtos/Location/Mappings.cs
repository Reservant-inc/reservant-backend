using AutoMapper;
using NetTopologySuite.Geometries;
using Reservant.Api.Mapping;

namespace Reservant.Api.Dtos.Location;

/// <summary>
/// Mapping profile for location DTOs
/// </summary>
public class Mappings : Profile
{
    /// <inheritdoc />
    public Mappings()
    {
        CreateMap<Point, Geolocation>()
            .MapMemberFrom(dto => dto.Latitude, point => point.Y)
            .MapMemberFrom(dto => dto.Longitude, point => point.X);
    }
}
