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

        CreateMap<User, UserEmployeeVM>()
            .MapMemberFrom(dto => dto.UserId, user => user.Id)
            .MapUploadPath(dto => dto.Photo, user => user.PhotoFileName, urlService)
            .MapMemberFrom(dto => dto.Login, user => user.UserName)
            .MapMemberFrom(dto => dto.Employments,
                user => user.Employments.Where(employment => employment.DateUntil == null));
    }
}
