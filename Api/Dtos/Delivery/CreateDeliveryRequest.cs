﻿using Reservant.Api.Models.Dtos.Ingredient;

namespace Reservant.Api.Dtos.Delivery;

public class CreateDeliveryRequest
{

    public required int RestaurantId { get; set; }

    public required List<IngredientDeliveryVM> Ingredients { get; init; }
}