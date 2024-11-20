using AutoMapper;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Deliveries;

/// <summary>
/// Mapping profiles for Delivery DTOs
/// </summary>
public class Mappings : Profile
{
    /// <inheritdoc />
    public Mappings()
    {
        CreateMap<Delivery, DeliveryVM>();
    }
}
