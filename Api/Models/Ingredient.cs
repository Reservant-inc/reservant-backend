﻿using Reservant.Api.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models;

/// <summary>
/// Concrete ingredient item
/// </summary>
public class Ingredient
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public int IngredientId { get; set; }

    /// <summary>
    /// Name of the ingredient item as shown to the customer
    /// </summary>
    [StringLength(20)]
    public required string PublicName { get; set; }

    /// <summary>
    /// Unit of measurement used for amount
    /// </summary>
    public UnitOfMeasurement UnitOfMeasurement { get; set; }

    /// <summary>
    /// Minimal amount considered enough
    /// </summary>
    public double MinimalAmount { get; set; }

    /// <summary>
    /// When added to the shopping list, the amount to order
    /// </summary>
    public double? AmountToOrder { get; set; }

    /// <summary>
    /// Current amount
    /// </summary>
    public double Amount { get; set; }

    /// <summary>
    /// ID of the restaurant
    /// </summary>
    public int RestaurantId { get; set; }

    /// <summary>
    /// Deliveries that resupplied the ingredient
    /// </summary>
    public ICollection<IngredientDelivery> Deliveries { get; set; } = null!;

    /// <summary>
    /// MenueItems ingrendiant is used in
    /// </summary>
    public ICollection<IngredientMenuItem> MenuItems { get; set; } = null!;

    /// <summary>
    /// Amount correction history
    /// </summary>
    public ICollection<IngredientAmountCorrection> Corrections { get; set; } = null!;

    /// <summary>
    /// Navigational property of restaurant
    /// </summary>
    public Restaurant Restaurant { get; set; } = null!;
}
