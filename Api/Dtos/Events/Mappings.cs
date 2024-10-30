using AutoMapper;
using NetTopologySuite.Geometries;
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
            .ForMember(dto => dto.Participants, builder =>
                builder.MapFrom(@event => @event.ParticipationRequests
                    .Where(request => request.DateAccepted != null)));

        CreateMap<Event, EventSummaryVM>()
            .MapMemberFrom(dto => dto.NumberInterested,
                @event => @event.ParticipationRequests.Count);

        CreateMap<ParticipationRequest, UserSummaryVM>()
            .IncludeMembers(request => request.User);

        Point? origin = null;
        CreateMap<Event, NearEventVM>()
            .MapMemberFrom(dto => dto.NumberInterested,
                @event => @event.ParticipationRequests.Count)
            .MapMemberFrom(dto => dto.NumberParticipants,
                @event => @event.ParticipationRequests.Count(request => request.DateAccepted != null))
            .MapMemberFrom(dto => dto.Distance,
                @event => @event.Restaurant == null || origin == null
                    ? null : (double?)@event.Restaurant.Location.Distance(origin));
    }
}
