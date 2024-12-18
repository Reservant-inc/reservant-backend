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
                visit => visit.NumberOfGuests + visit.Participants.Count + 1)
            .MapMemberFrom(dto => dto.Date,
                visit => visit.Reservation!.StartTime)
            .MapMemberFrom(dto => dto.EndTime,
                visit => visit.Reservation!.EndTime)
            .MapMemberFrom(dto => dto.ActualStartTime,
                visit => visit.StartTime)
            .MapMemberFrom(dto => dto.ActualEndTime,
                visit => visit.EndTime)
            .MapMemberFrom(dto => dto.Deposit,
                visit => visit.Reservation == null
                    ? null
                    : visit.Reservation.Deposit)
            .MapMemberFrom(dto => dto.IsCancelled,
                visit => visit.IsCancelled);

        CreateMap<Visit, VisitVM>()
            .MapMemberFrom(dto => dto.Date,
                visit => visit.Reservation!.StartTime)
            .MapMemberFrom(dto => dto.EndTime,
                visit => visit.Reservation!.EndTime)
            .MapMemberFrom(dto => dto.ActualStartTime,
                visit => visit.StartTime)
            .MapMemberFrom(dto => dto.ActualEndTime,
                visit => visit.EndTime)
            .MapMemberFrom(dto => dto.PaymentTime,
                visit => visit.Reservation == null
                    ? null : visit.Reservation.DepositPaymentTime)
            .MapMemberFrom(dto => dto.Deposit,
                visit => visit.Reservation == null
                    ? null : visit.Reservation.Deposit)
            .MapMemberFrom(dto => dto.ReservationDate,
                visit => visit.Reservation == null
                    ? null : visit.Reservation.ReservationDate)
            .MapMemberFrom(dto => dto.IsCancelled,
                visit => visit.IsCancelled);
    }
}
