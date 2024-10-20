using AutoMapper;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Messages;

/// <summary>
/// Mapping profile for message DTOs
/// </summary>
public class Mappings : Profile
{
    /// <inheritdoc />
    public Mappings()
    {
        CreateMap<Message, MessageVM>();
    }
}
