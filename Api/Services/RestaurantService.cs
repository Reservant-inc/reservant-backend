using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Restaurant;
using Reservant.Api.Models.Dtos.Table;
using Reservant.Api.Validation;
using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Services
{
    /// <param name="context">context</param>
    public class RestaurantService(ApiDbContext context, FileUploadService uploadService)
    {
        /// <summary>
        /// Register new Restaurant and add tables.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<Result<Restaurant>> CreateRestaurantAsync(CreateRestaurantRequest request, User user)
        {
            var errors = new List<ValidationResult>();

            RestaurantGroup? group;
            if (request.GroupId is null)
            {
                group = new RestaurantGroup
                {
                    Name = request.Name.Trim(),
                    OwnerId = user.Id
                };
                context.RestaurantGroups.Add(group);
            }
            else
            {
                group = await context.RestaurantGroups.FindAsync(request.GroupId);

                if (group is null)
                {
                    errors.Add(new ValidationResult(
                        $"Group with ID {request.GroupId} not found",
                        [nameof(request.GroupId)]));
                    return errors;
                }

                if (group.OwnerId != user.Id)
                {
                    errors.Add(new ValidationResult(
                        $"Group with ID {request.GroupId} is not owned by the current user",
                        [nameof(request.GroupId)]));
                    return errors;
                }
            }

            string? rentalContract = null;
            if (request.RentalContract is not null)
            {
                var result = await uploadService.ProcessUploadUriAsync(
                    request.RentalContract,
                    user.Id,
                    FileClass.Document,
                    nameof(request.RentalContract));
                if (result.IsError)
                {
                    return result.Errors;
                }

                rentalContract = result.Value;
            }

            string? alcoholLicense = null;
            if (request.AlcoholLicense is not null)
            {
                var result = await uploadService.ProcessUploadUriAsync(
                    request.AlcoholLicense,
                    user.Id,
                    FileClass.Document,
                    nameof(request.AlcoholLicense));
                if (result.IsError)
                {
                    return result.Errors;
                }

                alcoholLicense = result.Value;
            }

            var businessPermissionResult = await uploadService.ProcessUploadUriAsync(
                request.BusinessPermission,
                user.Id,
                FileClass.Document,
                nameof(request.BusinessPermission));
            if (businessPermissionResult.IsError)
            {
                return businessPermissionResult.Errors;
            }

            var idCardResult = await uploadService.ProcessUploadUriAsync(
                request.IdCard,
                user.Id,
                FileClass.Document,
                nameof(request.IdCard));
            if (idCardResult.IsError)
            {
                return idCardResult.Errors;
            }

            var restaurant = new Restaurant
            {
                Name = request.Name.Trim(),
                RestaurantType = request.RestaurantType,
                Address = request.Address.Trim(),
                Nip = request.Nip,
                PostalIndex = request.PostalIndex,
                City = request.City.Trim(),
                Group = group,
                RentalContractFileName = rentalContract,
                AlcoholLicenseFileName = alcoholLicense,
                BusinessPermissionFileName = businessPermissionResult.Value,
                IdCardFileName = idCardResult.Value
            };

            if (!ValidationUtils.TryValidate(restaurant, errors))
            {
                return errors;
            }

            context.Add(restaurant);

            await context.SaveChangesAsync();

            return restaurant;
        }
        /// <summary>
        /// Returns a list of restaurants owned by the user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<Result<List<RestaurantSummaryVM>>> GetMyRestaurantsAsync(User user) {
            var userId = user.Id;
            var result = await context.Restaurants.Where(r => r.Group!.OwnerId == userId)
                                                  .Select(r=> new RestaurantSummaryVM{
                                                    Id = r.Id,
                                                    Name = r.Name,
                                                    RestaurantType = r.RestaurantType,
                                                    Address = r.Address,
                                                    City = r.City
                                                  })
                                                  .ToListAsync();
            return result;
        }
        /// <summary>
        /// Returns a specific restaurant owned by the user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="id"> Id of the restaurant.</param>
        /// <returns></returns>
        public async Task<Result<RestaurantVM?>> GetMyRestaurantByIdAsync(User user, int id)
        {
            var userId = user.Id;
            var result = await context.Restaurants
                .Where(r => r.Group!.OwnerId == userId)
                .Where(r => r.Id == id)
                .Select(r => new RestaurantVM
                {
                    Id = r.Id,
                    Name = r.Name,
                    RestaurantType = r.RestaurantType,
                    Nip = r.Nip,
                    Address = r.Address,
                    PostalIndex = r.PostalIndex,
                    City = r.City,
                    GroupId = r.Group!.Id,
                    GroupName = r.Group!.Name,
                    RentalContract = r.RentalContractFileName == null
                        ? null : uploadService.GetPathForFileName(r.RentalContractFileName),
                    AlcoholLicense = r.AlcoholLicenseFileName == null
                        ? null : uploadService.GetPathForFileName(r.AlcoholLicenseFileName),
                    BusinessPermission = uploadService.GetPathForFileName(r.BusinessPermissionFileName),
                    IdCard = uploadService.GetPathForFileName(r.IdCardFileName),
                    Tables = r.Tables!.Select(t => new TableVM
                    {
                        Id = t.Id,
                        Capacity = t.Capacity
                    })
                })
                .FirstOrDefaultAsync();
            return result;
        }

        
        /// <summary>
        /// Returns a specific restaurant owned by the user.
        /// </summary>
        /// <param name="idUser"></param>
        /// <param name="idRestaurant"> Id of the restaurant.</param>
        /// <returns></returns>
        public async Task<bool> setVerifierIdAsync(User user, int idRestaurant)
        {
            var result = await context
            .Restaurants
            .Where(r => r.Id == idRestaurant)
            .AnyAsync();

            if(!result)
                return false;

            await context
            .Restaurants
            .Where (r => r.Id == idRestaurant)    
            .ForEachAsync(r =>
            {
                r.VerifierId=user.Id;
            });

            return true;
        }
    }
}
