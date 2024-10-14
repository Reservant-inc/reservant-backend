using AutoMapper;
using AutoMapper.Configuration.Annotations;
using NetTopologySuite.Geometries;

namespace Reservant.Api.Dtos.Location;

/// <summary>
/// Information geographical location
/// </summary>
[AutoMap(typeof(Point))]
public class Geolocation
{
    /// <summary>
    /// Restaurant Latitude
    /// </summary>
    /// <example>52.39625635</example>
    [SourceMember(nameof(Point.X))]
    public required double Latitude { get; init; }

    /// <summary>
    /// Restaurant Longitude
    /// </summary>
    /// <example>20.91364863552046</example>
    [SourceMember(nameof(Point.Y))]
    public required double Longitude { get; init; }

}
