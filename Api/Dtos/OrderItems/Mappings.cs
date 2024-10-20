using AutoMapper;
using Reservant.Api.Mapping;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.OrderItems;

/// <summary>
/// Mapping profile for order item DTOs
/// </summary>
public class Mappings : Profile
{
    /// <inheritdoc />
    public Mappings()
    {
        CreateMap<OrderItem, OrderItemVM>()
            .MapMemberFrom(dto => dto.Cost,
                orderItem => orderItem.Price * orderItem.Amount);
    }
}
