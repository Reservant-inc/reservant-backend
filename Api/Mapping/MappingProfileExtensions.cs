using System.Linq.Expressions;
using AutoMapper;

namespace Reservant.Api.Mapping;

/// <summary>
/// Extension methods to help write mapping profiles
/// </summary>
internal static class MappingProfileExtensions
{
    /// <summary>
    /// Map a member from an expression. Simplified version of:
    /// <code>
    /// .ForMember(..., b => b.MapFrom(...))
    /// </code>
    /// </summary>
    /// <param name="mapping">Mapping configuration expression in an AutoMapper profile</param>
    /// <param name="destinationMember">Member in the destination class to map</param>
    /// <param name="mapExpression">Expression to map the destination member to</param>
    /// <typeparam name="TSource">Type of the source class</typeparam>
    /// <typeparam name="TDestination">Type of the destination class</typeparam>
    /// <typeparam name="TMemberDestination">Type of the destination member</typeparam>
    /// <returns>The mapping configuration expression to continue the method chain</returns>
    internal static IMappingExpression<TSource, TDestination> MapMemberFrom<TSource, TDestination, TMemberDestination>(
        this IMappingExpression<TSource, TDestination> mapping,
        Expression<Func<TDestination, TMemberDestination>> destinationMember,
        Expression<Func<TSource, TMemberDestination>> mapExpression)
    {
        return mapping.ForMember(destinationMember, b =>
        {
            b.MapFrom(mapExpression);
        });
    }

    /// <summary>
    /// Map a member to the upload path of a file upload (see <see cref="UrlService.GetPathForFileName"/>)
    /// </summary>
    /// <param name="mapping">Mapping configuration expression in an AutoMapper profile</param>
    /// <param name="destinationMember">Member in the destination class to map</param>
    /// <param name="sourceFilenameMember">Member in the source class containing the file name</param>
    /// <param name="urlService"><see cref="UrlService"/></param>
    /// <typeparam name="TSource">Type of the source class</typeparam>
    /// <typeparam name="TDestination">Type of the destination class</typeparam>
    /// <returns>The mapping configuration expression to continue the method chain</returns>
    internal static IMappingExpression<TSource, TDestination> MapUploadPath<TSource, TDestination>(
        this IMappingExpression<TSource, TDestination> mapping,
        Expression<Func<TDestination, string?>> destinationMember,
        Expression<Func<TSource, string?>> sourceFilenameMember,
        UrlService urlService)
    {
        return mapping.ForMember(destinationMember, b =>
        {
            b.MapFrom(sourceFilenameMember);
            b.AddTransform(fileName => urlService.GetPathForFileName(fileName));
        });
    }
}
