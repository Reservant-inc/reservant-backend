using AutoMapper;
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
        CreateMap<Menu, MenuSummaryVM>();
    }
}
