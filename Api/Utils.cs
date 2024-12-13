using System.Diagnostics.CodeAnalysis;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Dtos;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.Api;

// Utils conflicts with the namespace Microsoft.Identity.Client.Utils
// which can be just ignored
#pragma warning disable CA1724

/// <summary>
/// Utility functions
/// </summary>
public static class Utils
{
    /// <summary>
    /// Convert a property path to camel case (example: 'PropertyName.Test' -> 'propertyName.test')
    /// </summary>
    /// <param name="str">The string to convert</param>
    public static string PropertyPathToCamelCase(string str) =>
        string.Join('.',
            str.Split('.')
                .Select(name => name.Length == 0
                    ? name
                    : char.ToLowerInvariant(name[0]) + name[1..]));

    private static readonly int[] NipWeights = [6, 5, 7, 2, 3, 4, 5, 6, 7];

    /// <summary>
    /// Check whether a string contains a valid
    /// <a href="https://pl.wikipedia.org/wiki/Numer_identyfikacji_podatkowej">NIP</a>
    /// </summary>
    public static bool IsValidNip(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length != 10 || !value.All(char.IsDigit))
        {
            return false;
        }

        var sum = NipWeights.Zip(value)
            .Select(vals => vals.First * (vals.Second - '0'))
            .Sum();

        var checksum = sum % 11;
        if (checksum == 10)
        {
            checksum = 0;
        }

        return checksum == value[9] - '0';
    }

    /// <summary>
    /// Check whether the value is a valid latitude
    /// </summary>
    public static bool IsValidLatitude(double lat) => lat is >= -180 and <= 180;

    /// <summary>
    /// Check whether the value is a valid longitude
    /// </summary>
    public static bool IsValidLongitude(double lon) => lon is >= -90 and <= 90;

    /// <summary>
    /// Return a single page of the query
    /// </summary>
    /// <param name="query">LINQ query</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Items per page</param>
    /// <param name="orderByOptions">Available sorting options (only used to return the info to the client)</param>
    /// <param name="maxPerPage">Maximum value for <paramref name="perPage"/></param>
    /// <param name="disablePagination">If set to true, return all items from the query</param>
    [ErrorCode(null, ErrorCodes.InvalidPerPageValue,
        "Must be >= 1 and <= maximum value, or skipping pagination is disallowed for this endpoint")]
    public static async Task<Result<Pagination<T>>> PaginateAsync<T>(
        this IQueryable<T> query,
        int page,
        int perPage,
        string[] orderByOptions,
        int maxPerPage = 100,
        bool disablePagination = false)
    {
        if (perPage == -1)
        {
            if (disablePagination)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "Disabling pagination is not allowed for this endpoint",
                    ErrorCode = ErrorCodes.InvalidPerPageValue,
                };
            }

            var allItems = await query.ToListAsync();
            return new Pagination<T>
            {
                Items = allItems,
                TotalPages = 1,
                Page = 0,
                PerPage = allItems.Count,
                OrderByOptions = orderByOptions,
            };
        }

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
                Items = new List<T>(),
                TotalPages = totalPages,
                Page = page,
                PerPage = perPage,
                OrderByOptions = orderByOptions
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
            PerPage = perPage,
            OrderByOptions = orderByOptions
        };
    }

}
