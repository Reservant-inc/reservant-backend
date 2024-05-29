using NetTopologySuite.Geometries;

namespace Reservant.Api.Models.Dtos.Location;

/// <summary>
/// Information geographical location
/// </summary>
public class Geolocation
{
    
    /// <summary>
    /// Restaurant Latitude
    /// </summary>
    /// <example>52.39625635</example>
    public required double Latitude { get; init; }
    
    /// <summary>
    /// Restaurant Longitude
    /// </summary>
    /// <example>20.91364863552046</example>
    public required double Longitude { get; init; }    
    
}