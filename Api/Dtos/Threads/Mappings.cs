using AutoMapper;

namespace Reservant.Api.Dtos.Threads;

/// <summary>
/// Mapping profile for thread DTOs
/// </summary>
public class Mappings : Profile
{
    /// <inheritdoc />
    public Mappings()
    {
        CreateMap<Thread, ThreadVM>();
    }
}
