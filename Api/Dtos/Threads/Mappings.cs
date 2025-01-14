using AutoMapper;
using Reservant.Api.Mapping;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Threads;

/// <summary>
/// Mapping profile for thread DTOs
/// </summary>
public class Mappings : Profile
{
    /// <inheritdoc />
    public Mappings()
    {
        CreateMap<MessageThread, ThreadVM>()
            .MapMemberFrom(dto => dto.ThreadId, thread => thread.MessageThreadId);
    }
}
