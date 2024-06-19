using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

namespace Reservant.Api;

/// <summary>
/// Utility functions
/// </summary>
public static class Utils
{
    /// <summary>
    /// Calculate distance (in meters) between two points on earth sphere
    /// </summary>
    /// <returns></returns>
    public static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula to calculate the distance in meters
        const double R = 6371e3; // Radius of Earth in meters
        var phi1 = lat1 * Math.PI / 180.0; // Convert latitude to radians
        var phi2 = lat2 * Math.PI / 180.0; // Convert latitude to radians
        var deltaPhi = (lat2 - lat1) * Math.PI / 180.0; // Difference in latitude in radians
        var deltaLambda = (lon2 - lon1) * Math.PI / 180.0; // Difference in longitude in radians

        var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                Math.Cos(phi1) * Math.Cos(phi2) *
                Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }
    
    /// <summary>
    /// Convert a property path to camel case (example: 'PropertyName.Test' -> 'propertyName.test')
    /// </summary>
    /// <param name="str">The string to convert</param>
    public static string PropertyPathToCamelCase(string str) =>
        string.Join('.',
            str.Split('.')
                .Select(name => name.Length == 0
                    ? name
                    : char.ToLower(name[0]) + name[1..]));

    /// <summary>
    /// Return a single page of the query
    /// </summary>
    public static async Task<Result<Pagination<T>>> PaginateAsync<T>(
        this IQueryable<T> query, int page, int perPage, int maxPerPage = 10)
    {
        if (perPage > maxPerPage)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = $"Too many items per page (Maximum: {maxPerPage})",
                ErrorCode = ErrorCodes.InvalidPerPageValue
            };
        }

        if (perPage < 1)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Items per page must be at least 1",
                ErrorCode = ErrorCodes.InvalidPerPageValue
            };
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalRecords / perPage);

        if (page < 0 || page >= totalPages)
        {
            return new Pagination<T>
            {
                Items = [],
                TotalPages = totalPages,
                Page = page,
                PerPage = perPage
            };
        }

        var paginatedResults = await query
            .Skip(page * perPage)
            .Take(perPage)
            .ToListAsync();

        return new Pagination<T>
        {
            Items = paginatedResults,
            TotalPages = totalPages,
            Page = page,
            PerPage = perPage
        };
    }
}
