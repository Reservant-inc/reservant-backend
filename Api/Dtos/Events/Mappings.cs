using AutoMapper;
using Reservant.Api.Dtos.Users;
using Reservant.Api.Mapping;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Events;

/// <summary>
/// Mapping profile for event DTOs
/// </summary>
public class Mappings : Profile
{
    /// <inheritdoc />
    public Mappings()
    {
        CreateMap<Event, EventVM>()
            .MapMemberFrom(dto => dto.CreatorFullName,
                @event => $"{@event.Creator.FirstName} {@event.Creator.LastName}")
            .ForMember(dto => dto.Participants, builder =>
                builder.MapFrom(@event => @event.ParticipationRequests
                    .Where(request => request.DateAccepted != null)));

        CreateMap<Event, EventSummaryVM>()
            .MapMemberFrom(dto => dto.CreatorFullName,
                @event => $"{@event.Creator.FirstName} {@event.Creator.LastName}")
            .MapMemberFrom(dto => dto.NumberInterested,
                @event => @event.ParticipationRequests.Count);

        CreateMap<ParticipationRequest, UserSummaryVM>()
            .IncludeMembers(request => request.User);
    }
}
