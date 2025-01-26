using AutoMapper;
using Reservant.Api.Mapping;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Menus;

/// <summary>
/// Mapping profile for menu DTOs
/// </summary>
public class Mappings : Profile
{
    /// <inheritdoc />
    public Mappings()
    {
        CreateMap<Menu, MenuVM>();
        CreateMap<Menu, MenuSummaryVM>()
            .MapMemberFrom(dto => dto.MenuItemIds,
                menu => menu.MenuItems.Select(mi => mi.MenuItemId));
    }
}
