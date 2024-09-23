using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Dtos;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.Api;

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
                    : char.ToLower(name[0]) + name[1..]));

    /// <summary>
    /// Return a single page of the query
    /// </summary>
    /// <param name="query">LINQ query</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Items per page</param>
    /// <param name="orderByOptions">Available sorting options (only used to return the info to the client)</param>
    /// <param name="maxPerPage">Maximum value for <paramref name="perPage"/></param>
    [ErrorCode(null, ErrorCodes.InvalidPerPageValue, "Must be >= 1 and <= maximum value")]
    public static async Task<Result<Pagination<T>>> PaginateAsync<T>(
        this IQueryable<T> query, int page, int perPage, string[] orderByOptions, int maxPerPage = 10)
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
