using AutoMapper;
using Reservant.Api.Mapping;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Visits;

/// <summary>
/// Mapping profile for visit DTOs
/// </summary>
public class Mappings : Profile
{
    /// <inheritdoc />
    public Mappings()
    {
        CreateMap<Visit, VisitSummaryVM>()
            .MapMemberFrom(dto => dto.NumberOfPeople,
                visit => visit.NumberOfGuests + visit.Participants.Count + 1);
    }
}
