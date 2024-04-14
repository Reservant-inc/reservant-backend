using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Restaurant;
using Reservant.Api.Models.Dtos.Table;
using Reservant.Api.Validation;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Identity;

namespace Reservant.Api.Services
{
    public class RestaurantService(ApiDbContext context, FileUploadService uploadService, UserManager<User> userManager)
    {
        /// <summary>
        /// Register new Restaurant and optionally a new group for it.
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

            var logoResult = await uploadService.ProcessUploadUriAsync(
                request.Logo,
                user.Id,
                FileClass.Image,
                nameof(request.Logo));
            if (logoResult.IsError)
            {
                return logoResult.Errors;
            }

            var tags = await context.RestaurantTags
                .Join(
                    request.Tags,
                    rt => rt.Name,
                    tag => tag,
                    (rt, _) => rt)
                .ToListAsync();

            var notFoundTags = request.Tags.Where(tag => tags.All(rt => rt.Name != tag));
            errors.AddRange(notFoundTags.Select(
                tag => new ValidationResult($"Tag not found: {tag}", [nameof(request.Tags)])));
            if (errors.Count != 0)
            {
                return errors;
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
                IdCardFileName = idCardResult.Value,
                LogoFileName = logoResult.Value,
                ProvideDelivery = request.ProvideDelivery,
                Description = request.Description?.Trim(),
                Tags = tags
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
        public async Task<List<RestaurantSummaryVM>> GetMyRestaurantsAsync(User user) {
            var userId = user.Id;
            var result = await context.Restaurants
                .Where(r => r.Group!.OwnerId == userId)
                .Select(r=> new RestaurantSummaryVM
                {
                    Id = r.Id,
                    Name = r.Name,
                    RestaurantType = r.RestaurantType,
                    Address = r.Address,
                    City = r.City,
                    GroupId = r.GroupId,
                    ProvideDelivery = r.ProvideDelivery,
                    Logo = uploadService.GetPathForFileName(r.LogoFileName),
                    Description = r.Description,
                    Tags = r.Tags!.Select(t => t.Name).ToList()
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
        public async Task<RestaurantVM?> GetMyRestaurantByIdAsync(User user, int id)
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
                    }),
                    Photos = r.Photos!
                        .OrderBy(rp => rp.Order)
                        .Select(rp => uploadService.GetPathForFileName(rp.PhotoFileName))
                        .ToList(),
                    ProvideDelivery = r.ProvideDelivery,
                    Logo = uploadService.GetPathForFileName(r.LogoFileName),
                    Description = r.Description,
                    Tags = r.Tags!.Select(t => t.Name).ToList()
                })
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            return result;
        }

        /// <summary>
        /// Add the given employee to the given restaurant, acting as the given employer (restaurant owner)
        /// </summary>
        /// <param name="restaurantId">ID of the restaurant to add the employee to</param>
        /// <param name="employeeId">ID of the employee to add</param>
        /// <param name="employerId">ID of the current user (restaurant owner)</param>
        /// <returns>The bool returned inside the result does not mean anything</returns>
        public async Task<Result<bool>> AddEmployeeAsync(AddEmployeeRequest request, int restaurantId, string employerId)
        {
            if (!request.IsBackdoorEmployee && !request.IsHallEmployee)
            {
                return new List<ValidationResult>
                {
                    new($"Employee must have at least one role",
                        [nameof(request.IsBackdoorEmployee), nameof(request.IsHallEmployee)])
                };
            }

            var restaurantOwnerId = await context.Restaurants
                .Where(r => r.Id == restaurantId)
                .Select(r => r.Group!.OwnerId)
                .FirstOrDefaultAsync();
            if (restaurantOwnerId is null)
            {
                return new List<ValidationResult>
                {
                    new($"Restaurant with ID {restaurantId} not found")
                };
            }

            if (restaurantOwnerId != employerId)
            {
                return new List<ValidationResult>
                {
                    new("User is not the owner of the restaurant")
                };
            }

            var employee = await context.Users.FindAsync(request.EmployeeId);
            if (employee is null)
            {
                return new List<ValidationResult>
                {
                    new($"User with ID {request.EmployeeId} not found",
                        [nameof(request.EmployeeId)])
                };
            }

            if (!await userManager.IsInRoleAsync(employee, Roles.RestaurantEmployee)
                || employee.EmployerId != employerId)
            {
                return new List<ValidationResult>
                {
                    new($"User with ID {request.EmployeeId} is not current user's employee",
                        [nameof(request.EmployeeId)])
                };
            }

            var existingEmployment = await context.Employments.FindAsync(request.EmployeeId, restaurantId);
            if (existingEmployment is not null)
            {
                existingEmployment.IsHallEmployee = request.IsHallEmployee;
                existingEmployment.IsBackdoorEmployee = request.IsBackdoorEmployee;
            }
            else
            {
                context.Employments.Add(new Employment
                {
                    EmployeeId = request.EmployeeId,
                    RestaurantId = restaurantId,
                    IsBackdoorEmployee = request.IsBackdoorEmployee,
                    IsHallEmployee = request.IsHallEmployee
                });
            }

            await context.SaveChangesAsync();
            return true;
        }
    }
}
