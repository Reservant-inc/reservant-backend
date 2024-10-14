using AutoMapper;
using Reservant.Api.Mapping;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.RestaurantGroups;

/// <summary>
/// Mapping profile for restaurant group DTOs
/// </summary>
public class Mappings : Profile
{
    /// <inheritdoc />
    public Mappings()
    {
        CreateMap<RestaurantGroup, RestaurantGroupSummaryVM>()
            .MapMemberFrom(dto => dto.RestaurantCount, group => group.Restaurants.Count);
    }
}
