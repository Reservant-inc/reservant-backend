using AutoMapper;
using Reservant.Api.Mapping;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Users;

/// <summary>
/// Mapping profile for user DTOs
/// </summary>
public class Mappings : Profile
{
    /// <inheritdoc />
    public Mappings(UrlService urlService)
    {
        CreateMap<User, UserSummaryVM>()
            .MapMemberFrom(dto => dto.UserId, user => user.Id)
            .MapUploadPath(dto => dto.Photo, user => user.PhotoFileName, urlService);
    }
}
