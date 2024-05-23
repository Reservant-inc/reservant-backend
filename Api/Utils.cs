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
        this IQueryable<T> query, int page, int perPage, int maxPerPage)
    {
        if (perPage > maxPerPage)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = $"Too many items per page (Maximum: {maxPerPage})",
                ErrorCode = ErrorCodes.InvalidPageOrPerPageValue
            };
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalRecords / perPage);

        if (page < 0 || perPage <= 0 || page >= totalPages)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Invalid page or perPage value",
                ErrorCode = ErrorCodes.InvalidPageOrPerPageValue
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
