using AutoMapper;
using Reservant.Api.Mapping;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.MenuItems;

/// <summary>
/// Mapping profile for menu item DTOs
/// </summary>
public class Mappings : Profile
{
    /// <inheritdoc />
    public Mappings(UrlService urlService)
    {
        CreateMap<MenuItem, MenuItemSummaryVM>()
            .MapUploadPath(dto => dto.Photo,
                menuItem => menuItem.PhotoFileName, urlService);

        CreateMap<MenuItem, MenuItemVM>()
            .MapUploadPath(dto => dto.Photo,
                menuItem => menuItem.PhotoFileName, urlService);
    }
}
