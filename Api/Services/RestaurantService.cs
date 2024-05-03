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
using Reservant.Api.Models.Dtos.Menu;
using Reservant.Api.Models.Dtos.MenuItem;
using Microsoft.AspNetCore.Mvc;

namespace Reservant.Api.Services
{
    public enum VerificationResult
    {
        RestaurantNotFound,
        VerifierAlreadyExists,
        VerifierSetSuccessfully,
    }

    public class RestaurantService(ApiDbContext context, FileUploadService uploadService, UserManager<User> userManager,MenuItemsService menuItemsServiceservice)
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
                    Nip = r.Nip,
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

            var currentEmployment = await context.Employments
                .Where(e => e.EmployeeId == request.Id && e.RestaurantId == restaurantId && e.DateUntil == null)
                .FirstOrDefaultAsync();

            if (currentEmployment != null)
            {
                return new List<ValidationResult>
                {
                    new ("Employee is currently employed at this restaurant")
                };
            }

            context.Employments.Add(new Employment
            {
                EmployeeId = request.Id,
                RestaurantId = restaurantId,
                IsBackdoorEmployee = request.IsBackdoorEmployee,
                IsHallEmployee = request.IsHallEmployee,
                DateFrom = DateOnly.FromDateTime(DateTime.Now)
            });

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
                Nip = restaurant.Nip,
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
        /// Get list of restaurant's current employees
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

            if (restaurant == null)
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
                .Where(e => e.DateUntil == null)
                .Select(e => new RestaurantEmployeeVM
                {
                    EmploymentId = e.Id,
                    Id = e.EmployeeId,
                    Login = e.Employee!.UserName!,
                    FirstName = e.Employee.FirstName,
                    LastName = e.Employee.LastName,
                    PhoneNumber = e.Employee.PhoneNumber!,
                    IsBackdoorEmployee = e.IsBackdoorEmployee,
                    IsHallEmployee = e.IsHallEmployee,
                    DateFrom = e.DateFrom,
                    DateUntil = e.DateUntil
                })
                .ToList();
        }



        /// <summary>
        /// Updates restaurant info
        /// </summary>
        /// <param name="id">ID of the restaurant</param>
        /// <param name="request">Request with new restaurant data</param>
        /// <param name="user">User requesting a update</param>
        public async Task<Result<RestaurantVM>> UpdateRestaurantAsync(int id, UpdateRestaurantRequest request, User user)
        {
            var errors = new List<ValidationResult>();

            var restaurant = await context.Restaurants
                .Include(restaurant => restaurant.Group)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
            {
                errors.Add(new ValidationResult($"Restaurant with ID {id} not found."));
                return errors;
            }

            if (restaurant.Group?.OwnerId != user.Id)
            {
                errors.Add(new ValidationResult("User is not the owner of this restaurant."));
                return errors;
            }

            // Process file uploads directly
            string? rentalContract = null;
            if (request.RentalContract != null)
            {
                var result = await uploadService.ProcessUploadNameAsync(request.RentalContract, user.Id, FileClass.Document, nameof(request.RentalContract));
                if (result.IsError)
                {
                    errors.AddRange(result.Errors);
                }
                else
                {
                    rentalContract = result.Value;
                }
            }

            string? alcoholLicense = null;
            if (request.AlcoholLicense != null)
            {
                var result = await uploadService.ProcessUploadNameAsync(request.AlcoholLicense, user.Id, FileClass.Document, nameof(request.AlcoholLicense));
                if (result.IsError)
                {
                    errors.AddRange(result.Errors);
                }
                else
                {
                    alcoholLicense = result.Value;
                }
            }

            var businessPermissionResult = await uploadService.ProcessUploadNameAsync(request.BusinessPermission, user.Id, FileClass.Document, nameof(request.BusinessPermission));
            if (businessPermissionResult.IsError)
            {
                errors.AddRange(businessPermissionResult.Errors);
            }

            var idCardResult = await uploadService.ProcessUploadNameAsync(request.IdCard, user.Id, FileClass.Document, nameof(request.IdCard));
            if (idCardResult.IsError)
            {
                errors.AddRange(idCardResult.Errors);
            }

            var logoResult = await uploadService.ProcessUploadNameAsync(request.Logo, user.Id, FileClass.Image, nameof(request.Logo));
            if (logoResult.IsError)
            {
                errors.AddRange(logoResult.Errors);
            }

            if (errors.Any())
            {
                return errors;
            }

            restaurant.Name = request.Name;
            restaurant.Nip = request.Nip;
            restaurant.RestaurantType = request.RestaurantType;
            restaurant.Address = request.Address;
            restaurant.PostalIndex = request.PostalIndex;
            restaurant.City = request.City;
            restaurant.ProvideDelivery = request.ProvideDelivery;
            restaurant.Description = request.Description;

            restaurant.RentalContractFileName = rentalContract;
            restaurant.AlcoholLicenseFileName = alcoholLicense;
            restaurant.BusinessPermissionFileName = businessPermissionResult.Value;
            restaurant.IdCardFileName = idCardResult.Value;
            restaurant.LogoFileName = logoResult.Value;


            restaurant.Tags = await context.RestaurantTags
                .Where(t => request.Tags.Contains(t.Name))
                .ToListAsync();


            // Update photos
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

            restaurant.Photos!.Clear();
            foreach (var photo in photos)
            {
                restaurant.Photos.Add(photo);
            }

            if (!ValidationUtils.TryValidate(restaurant, errors))
            {
                return errors;
            }

            await context.SaveChangesAsync();
            var newRestaurant = await context.Restaurants
                .Where(r => r.Group!.OwnerId == user.Id)
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

            return newRestaurant;
        }


        /// <summary>
        /// Returns a specific restaurant owned by the user.
        /// </summary>
        /// <param name="idUser"></param>
        /// <param name="idRestaurant"> Id of the restaurant.</param>
        /// <returns></returns>
        public async Task<VerificationResult> SetVerifiedIdAsync(User user, int idRestaurant)
        {
            var result = await context
                .Restaurants
                .Where(r => r.Id == idRestaurant)
                .FirstOrDefaultAsync();

            if (result is null)
                return VerificationResult.RestaurantNotFound;

            if (result.VerifierId is not null)
                return VerificationResult.VerifierAlreadyExists;

            result.VerifierId = user.Id;
            await context.SaveChangesAsync();

            return VerificationResult.VerifierSetSuccessfully;
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
        /// Returns a list of menus of specific restaurant
        /// </summary>
        /// <param name="id"> Id of the restaurant.</param>
        /// <returns></returns>
        public async Task<List<MenuSummaryVM>> GetMenusAsync(int id)
        {
            var restaurant = await context.Restaurants
                .Include(r => r.Menus)
                    .ThenInclude(m => m.MenuItems)
                .Where(i => i.Id == id)
                .FirstOrDefaultAsync();

            if (restaurant == null)
            {
                return null;
            }

            var menus = restaurant.Menus
                .Select(menu => new MenuSummaryVM
                {
                    Id = menu.Id,
                    MenuType = menu.MenuType,
                    DateFrom = menu.DateFrom,
                    DateUntil = menu.DateUntil
                })
                .ToList();

            return menus;
        }


        /// <summary>
        /// Validates and gets menu items from the given restaurant
        /// </summary>
        /// <param name="user"></param>
        /// <param name="restaurantId"></param>
        /// <returns>MenuItems</returns>
        public async Task<Result<List<MenuItemVM>>> GetMenuItemsAsync(User user, int restaurantId)
        {
            var errors = new List<ValidationResult>();

            var isRestaurantValid = await menuItemsServiceservice.ValidateRestaurant(user, restaurantId);

            if (isRestaurantValid.IsError)
            {
                return isRestaurantValid.Errors;
            }

            return await context.MenuItems
                .Where(i => i.RestaurantId == restaurantId)
                .Select(i => new MenuItemVM()
                {
                    Id = i.Id,
                    Name = i.Name,
                    Price = i.Price,
                    AlcoholPercentage = i.AlcoholPercentage,
                }).ToListAsync();

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
                .AsSplitQuery()
                .Include(r => r.Group!)
                .ThenInclude(g => g.Restaurants)
                .Include(restaurant => restaurant.Tables!)
                .Include(restaurant => restaurant.Employments!)
                .Include(restaurant => restaurant.Photos!)
                .Include(restaurant => restaurant.Menus!)
                .Include(restaurant => restaurant.MenuItems!)
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

            context.RemoveRange(restaurant.Tables!);
            context.RemoveRange(restaurant.Employments!);
            context.RemoveRange(restaurant.Photos!);
            context.RemoveRange(restaurant.Menus!);
            context.RemoveRange(restaurant.MenuItems!);

            await context.SaveChangesAsync();
            return true;
        }

    }
}
