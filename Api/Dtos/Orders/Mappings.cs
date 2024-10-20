using AutoMapper;
using Reservant.Api.Mapping;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Orders;

/// <summary>
/// Mapping profile for order DTOs
/// </summary>
public class Mappings : Profile
{
    /// <inheritdoc />
    public Mappings()
    {
        CreateMap<Order, OrderSummaryVM>()
            .MapMemberFrom(dto => dto.Cost,
                order => order.OrderItems.Sum(oi => oi.Price * oi.Amount))
            .MapMemberFrom(dto => dto.Status, order =>
                order.OrderItems.Select(oi => oi.Status).MaxBy(s => (int)s));

        CreateMap<Order, OrderVM>()
            .MapMemberFrom(dto => dto.Cost,
                order => order.OrderItems.Sum(oi => oi.Price * oi.Amount))
            .MapMemberFrom(dto => dto.Status, order =>
                order.OrderItems.Select(oi => oi.Status).MaxBy(s => (int)s));
    }
}
