using AutoMapper;
using Reservant.Api.Mapping;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Restaurants;

/// <summary>
/// Mapping profile for restaurant DTOs
/// </summary>
public class Mappings : Profile
{
    /// <inheritdoc />
    public Mappings(UrlService urlService)
    {
        CreateMap<Restaurant, MyRestaurantSummaryVM>()
            .MapUploadPath(dto => dto.Logo,
                restaurant => restaurant.LogoFileName, urlService)
            .MapMemberFrom(dto => dto.IsVerified,
                restaurant => restaurant.VerifierId != null)
            .MapMemberFrom(dto => dto.Rating,
                restaurant => restaurant.Reviews.Average(review => (double?)review.Stars) ?? 0)
            .MapMemberFrom(dto => dto.NumberReviews,
                restaurant => restaurant.Reviews.Count)
            .MapMemberFrom(dto => dto.Tags,
                restaurant => restaurant.Tags.Select(tag => tag.Name));

        CreateMap<Restaurant, RestaurantSummaryVM>()
            .MapUploadPath(dto => dto.Logo,
                restaurant => restaurant.LogoFileName, urlService)
            .MapMemberFrom(dto => dto.Rating,
                restaurant => restaurant.Reviews.Average(review => (double?)review.Stars) ?? 0)
            .MapMemberFrom(dto => dto.NumberReviews,
                restaurant => restaurant.Reviews.Count)
            .MapMemberFrom(dto => dto.Tags,
                restaurant => restaurant.Tags.Select(tag => tag.Name));

        CreateMap<Restaurant, MyRestaurantVM>()
            .MapUploadPath(dto => dto.RentalContract,
                restaurant => restaurant.RentalContractFileName, urlService)
            .MapUploadPath(dto => dto.AlcoholLicense,
                restaurant => restaurant.AlcoholLicenseFileName, urlService)
            .MapUploadPath(dto => dto.BusinessPermission,
                restaurant => restaurant.BusinessPermissionFileName, urlService)
            .MapUploadPath(dto => dto.IdCard,
                restaurant => restaurant.IdCardFileName, urlService)
            .MapUploadPath(dto => dto.Logo,
                restaurant => restaurant.LogoFileName, urlService)
            .MapMemberFrom(dto => dto.Photos,
                restaurant => restaurant.Photos
                    .Select(photo => urlService.GetPathForFileName(photo.PhotoFileName))
                    .ToList())
            .MapMemberFrom(dto => dto.Tags,
                restaurant => restaurant.Tags.Select(tag => tag.Name))
            .MapMemberFrom(dto => dto.IsVerified,
                restaurant => restaurant.VerifierId != null);

        CreateMap<Restaurant, RestaurantVM>()
            .MapUploadPath(dto => dto.Logo,
                restaurant => restaurant.LogoFileName, urlService)
            .MapMemberFrom(dto => dto.Photos,
                restaurant => restaurant.Photos
                    .Select(photo => urlService.GetPathForFileName(photo.PhotoFileName))
                    .ToList())
            .MapMemberFrom(dto => dto.Tags,
                restaurant => restaurant.Tags.Select(tag => tag.Name))
            .MapMemberFrom(dto => dto.Rating,
                restaurant => restaurant.Reviews.Average(review => (double?)review.Stars) ?? 0)
            .MapMemberFrom(dto => dto.NumberReviews,
                restaurant => restaurant.Reviews.Count);

        CreateMap<OpeningHours, AvailableHoursVM>()
            .MapMemberFrom(oh => oh.From,
                ah => ah.From)
            .MapMemberFrom(oh => oh.Until,
                ah => ah.Until);
    }
}
