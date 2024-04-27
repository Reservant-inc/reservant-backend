using Microsoft.AspNetCore.Http.HttpResults;
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
                var result = await uploadService.ProcessUploadNameAsync(
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
                var result = await uploadService.ProcessUploadNameAsync(
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

            var businessPermissionResult = await uploadService.ProcessUploadNameAsync(
                request.BusinessPermission,
                user.Id,
                FileClass.Document,
                nameof(request.BusinessPermission));
            if (businessPermissionResult.IsError)
            {
                return businessPermissionResult.Errors;
            }

            var idCardResult = await uploadService.ProcessUploadNameAsync(
                request.IdCard,
                user.Id,
                FileClass.Document,
                nameof(request.IdCard));
            if (idCardResult.IsError)
            {
                return idCardResult.Errors;
            }

            var logoResult = await uploadService.ProcessUploadNameAsync(
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

            var photos = new List<RestaurantPhoto>();
            foreach (var (photo, order) in request.Photos.Select((photo, index) => (photo, index + 1)))
            {
                var result = await uploadService.ProcessUploadNameAsync(
                    photo,
                    user.Id,
                    FileClass.Image,
                    nameof(request.Photos));
                if (result.IsError)
                {
                    return result.Errors;
                }

                photos.Add(new RestaurantPhoto
                {
                    PhotoFileName = result.Value,
                    Order = order
                });
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
                Tags = tags,
                Photos = photos
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
        public async Task<List<RestaurantSummaryVM>> GetMyRestaurantsAsync(User user)
        {
            var userId = user.Id;
            var result = await context.Restaurants
                .Where(r => r.Group!.OwnerId == userId)
                .Select(r => new RestaurantSummaryVM
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
                    Tags = r.Tags!.Select(t => t.Name).ToList(),
                    IsVerified = r.VerifierId != null
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
                    Tags = r.Tags!.Select(t => t.Name).ToList(),
                    IsVerified = r.VerifierId != null
                })
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            return result;
        }

        /// <summary>
        /// Add the given employee to the given restaurant, acting as the given employer (restaurant owner)
        /// </summary>
        /// <param name="restaurantId">ID of the restaurant to add the employee to</param>
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

            var employee = await context.Users.FindAsync(request.Id);
            if (employee is null)
            {
                return new List<ValidationResult>
                {
                    new($"User with ID {request.Id} not found",
                        [nameof(request.Id)])
                };
            }

            if (!await userManager.IsInRoleAsync(employee, Roles.RestaurantEmployee)
                || employee.EmployerId != employerId)
            {
                return new List<ValidationResult>
                {
                    new($"User with ID {request.Id} is not current user's employee",
                        [nameof(request.Id)])
                };
            }

            var existingEmployment = await context.Employments.FindAsync(request.Id, restaurantId);
            if (existingEmployment is not null)
            {
                existingEmployment.IsHallEmployee = request.IsHallEmployee;
                existingEmployment.IsBackdoorEmployee = request.IsBackdoorEmployee;
            }
            else
            {
                context.Employments.Add(new Employment
                {
                    EmployeeId = request.Id,
                    RestaurantId = restaurantId,
                    IsBackdoorEmployee = request.IsBackdoorEmployee,
                    IsHallEmployee = request.IsHallEmployee
                });
            }

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<Result<RestaurantSummaryVM>> MoveRestaurantToGroupAsync(int restaurantId, MoveToGroupRequest request, User user)
        {
            var errors = new List<ValidationResult>();
            var newRestaurantGroup = await context.RestaurantGroups.Include(rg => rg.Restaurants).FirstOrDefaultAsync(rg => rg.Id == request.GroupId && rg.OwnerId == user.Id);
            if (newRestaurantGroup == null)
            {
                errors.Add(new ValidationResult(
                    $"RestaurantGroup with ID {request.GroupId} not found.",
                        [nameof(request.GroupId)]));
                return errors;
            }
            var restaurant = await context.Restaurants
                .Include(r => r.Tags)
                .Include(r => r.Group)
                .ThenInclude(g => g.Restaurants)
                .FirstOrDefaultAsync(r => r.Id == restaurantId && r.Group.OwnerId == user.Id);
            if (restaurant == null)
            {
                errors.Add(new ValidationResult(
                    $"Restaurant with ID {restaurantId} not found.",
                        [nameof(restaurantId)]));
                return errors;
            }

            var oldGroup = restaurant.Group;
            oldGroup.Restaurants.Remove(restaurant);
            if (oldGroup.Restaurants.Count == 0)
            {
                context.Remove(oldGroup);
            }
            restaurant.GroupId = request.GroupId;
            restaurant.Group = newRestaurantGroup;
            newRestaurantGroup.Restaurants.Add(restaurant);
            var result = context.RestaurantGroups.Update(newRestaurantGroup);
            await context.SaveChangesAsync();
            return new RestaurantSummaryVM
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                RestaurantType = restaurant.RestaurantType,
                Address = restaurant.Address,
                City = restaurant.City,
                GroupId = restaurant.GroupId,
                Description = restaurant.Description,
                Logo = uploadService.GetPathForFileName(restaurant.LogoFileName),
                Tags = restaurant.Tags!.Select(t => t.Name).ToList(),
                ProvideDelivery = restaurant.ProvideDelivery,
                IsVerified = restaurant.VerifierId != null
            };
        }

        /// <summary>
        /// Get list of restaurant's employees
        /// </summary>
        /// <param name="id">ID of the restaurants</param>
        /// <param name="userId">ID of the current user (to check permissions)</param>
        public async Task<Result<List<RestaurantEmployeeVM>>> GetEmployeesAsync(int id, string userId)
        {
            var restaurant = await context.Restaurants
                .Include(r => r.Group)
                .Include(r => r.Employments!)
                .ThenInclude(e => e.Employee)
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();
            if (restaurant is null)
            {
                return new List<ValidationResult>
                {
                    new($"Restaurant with ID {id} not found")
                };
            }

            if (restaurant.Group!.OwnerId != userId)
            {
                return new List<ValidationResult>
                {
                    new($"Restaurant with ID {id} is not owned by the current user")
                };
            }

            return restaurant.Employments!
                .Select(e => new RestaurantEmployeeVM
                {
                    Id = e.EmployeeId,
                    Login = e.Employee!.UserName!,
                    FirstName = e.Employee.FirstName,
                    LastName = e.Employee.LastName,
                    PhoneNumber = e.Employee.PhoneNumber!,
                    IsBackdoorEmployee = e.IsBackdoorEmployee,
                    IsHallEmployee = e.IsHallEmployee
                })
                .ToList();
        }


        /// <summary>
        /// Returns a specific restaurant owned by the user.
        /// </summary>
        /// <param name="idUser"></param>
        /// <param name="idRestaurant"> Id of the restaurant.</param>
        /// <returns></returns>
        public async Task<bool> SetVerifierIdAsync(User user, int idRestaurant)
        {
            var result = await context
            .Restaurants
            .Where(r => r.Id == idRestaurant)
            .AnyAsync();

            if (!result)
                return false;

            await context
            .Restaurants
            .Where(r => r.Id == idRestaurant)
            .ForEachAsync(r =>
            {
                r.VerifierId = user.Id;
            });
            await context.SaveChangesAsync();

            return true;
        }
        /// <summary>
        /// Validates if given dto is valid. If a group is given, checks if that group belongs to User
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<Result<bool>> ValidateFirstStepAsync(ValidateRestaurantFirstStepRequest dto, User user)
        {
            var errors = new List<ValidationResult>();

            if (!ValidationUtils.TryValidate(dto, errors))
            {
                return errors;
            }

            if (dto.GroupId != null)
            {
                var group = await context.RestaurantGroups.FindAsync(dto.GroupId);

                if (group is null)
                {
                    errors.Add(new ValidationResult(
                        $"Group with ID {dto.GroupId} not found",
                        [nameof(dto.GroupId)]));
                    return errors;
                }

                if (group.OwnerId != user.Id)
                {
                    errors.Add(new ValidationResult(
                        $"Group with ID {dto.GroupId} is not owned by the current user",
                        [nameof(dto.GroupId)]));
                    return errors;
                }

            }

            return true;

        }

        /// <summary>
        /// Function for soft deleting Restaurants that also deletes newly emptied restaurant groups
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool> SoftDeleteRestaurantAsync(int id, User user)
        {
            var restaurant = await context.Restaurants
                .Include(r => r.Group!)
                .ThenInclude(g => g.Restaurants)
                .Where(r => r.Id == id && r.Group!.OwnerId == user.Id)
                .FirstOrDefaultAsync();
            if (restaurant == null)
            {
                return false;
            }

            context.Remove(restaurant);
            if (restaurant.Group!.Restaurants!.Count == 0)
            {
                context.Remove(restaurant.Group);
            }
            await context.SaveChangesAsync();
            return true;
        }
    }
}
