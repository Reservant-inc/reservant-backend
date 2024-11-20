using AutoMapper;
using Reservant.Api.Mapping;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Ingredients;

/// <summary>
/// Mapping profile for ingredient DTOs
/// </summary>
public class Mappings : Profile
{
    /// <inheritdoc />
    public Mappings()
    {
        CreateMap<IngredientMenuItem, MenuItemIngredientVM>()
            .MapMemberFrom(dto => dto.PublicName, imi => imi.Ingredient.PublicName);

        CreateMap<IngredientAmountCorrection, IngredientAmountCorrectionVM>()
            .MapMemberFrom(dto => dto.CorrectionId, correction => correction.Id);

        CreateMap<IngredientDelivery, IngredientDeliveryVM>();
    }
}
