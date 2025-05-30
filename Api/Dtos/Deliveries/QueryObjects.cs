﻿namespace Reservant.Api.Dtos.Deliveries;

/// <summary>
/// Reusable parts of queries
/// </summary>
public static class QueryObjects
{
    /// <summary>
    /// Convert to DeliverySummaryVM
    /// </summary>
    public static IQueryable<DeliverySummaryVM> AsDeliverySummary(this IQueryable<Models.Delivery> query)
    {
        return query.Select(d => new DeliverySummaryVM
        {
            DeliveryId = d.DeliveryId,
            OrderTime = d.OrderTime,
            DeliveredTime = d.DeliveredTime,
            CanceledTime = d.CanceledTime,
            UserId = d.UserId,
            UserFullName = d.User == null ? null : d.User.FirstName + " " + d.User.LastName,
            Cost = d.Ingredients.Sum(i => (decimal)i.AmountOrdered),
        });
    }
}
