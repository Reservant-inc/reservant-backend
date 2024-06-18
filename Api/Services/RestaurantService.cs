using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Restaurant;
using Reservant.Api.Models.Dtos.Table;
using Reservant.Api.Validation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;
using Reservant.Api.Identity;
using Reservant.Api.Models.Dtos.Menu;
using Reservant.Api.Models.Dtos.MenuItem;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Models.Dtos.Location;
using Reservant.Api.Models.Dtos.Order;
using Reservant.Api.Models.Enums;
using Reservant.Api.Validators;



namespace Reservant.Api.Services
{
    /// <summary>
    /// Indicates the status code returned by <see cref="RestaurantService.SetVerifiedIdAsync"/>
    /// </summary>
    public enum VerificationResult
    {
        /// <summary>
        /// Restaurant not found
        /// </summary>
        RestaurantNotFound,

        /// <summary>
        /// Restaurant is already verified
        /// </summary>
        VerifierAlreadyExists,

        /// <summary>
        /// Success
        /// </summary>
        VerifierSetSuccessfully,
    }

    /// <summary>
    /// Service responsible for managing restaurants
    /// </summary>
    public class RestaurantService(
        ApiDbContext context,
        FileUploadService uploadService,
        UserManager<User> userManager,
        MenuItemsService menuItemsService,
        ValidationService validationService)
    {
        
        
        /// <summary>
        /// Finds restaurant in a given radius.
        /// </summary>
        /// <param name="lat">Latitude</param>
        /// <param name="lon">Longitude</param>
        /// <param name="radius">Radius in kilometers</param>
        /// <param name="user">User calling method</param>
        /// <returns></returns>
        public async Task<Result<List<NearRestaurantVM>>> GetRestaurantsAsync(double lat, double lon, int radius)
        {
            // converting to meters
            radius *= 1000;
            
            if (radius <= 0)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(radius),
                    ErrorMessage = "Radius cannot be less than one.",
                    ErrorCode = ErrorCodes.ValueLessThanOne
                };
            }

            const int limit = 20;

            if (radius > limit*1000)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(radius),
                    ErrorMessage = $"Radius cannot exceed search limit ({limit}km).",
                    ErrorCode = ErrorCodes.ValueExceedsLimit
                };
            }

            var restaurants = await context.Restaurants
                .Include(r => r.Group)
                .Include(r => r.Tables)
                .Include(r => r.Photos)
                .Include(r => r.Tags)
                .ToListAsync();

            var nearRestaurants = new List<NearRestaurantVM>();

            foreach (var r in restaurants)
            {
                var restaurantPoint = r.Location;

                var distance = Utils.CalculateHaversineDistance(
                    lon, 
                    lat, 
                    restaurantPoint.Y, 
                    restaurantPoint.X
                    );

                if (distance <= radius)
                {
                    nearRestaurants.Add(new NearRestaurantVM
                    {
                        RestaurantId = r.Id,
                        Name = r.Name,
                        RestaurantType = r.RestaurantType,
                        Nip = r.Nip,
                        Address = r.Address,
                        PostalIndex = r.PostalIndex,
                        City = r.City,
                        Location = new Geolocation
                        {
                            Latitude = r.Location.Y,
                            Longitude = r.Location.X
                        },
                        GroupId = r.GroupId,
                        GroupName = r.Group.Name,
                        RentalContract = r.RentalContractFileName is not null ? uploadService.GetPathForFileName(r.RentalContractFileName) : null,
                        AlcoholLicense = r.AlcoholLicenseFileName is not null ? uploadService.GetPathForFileName(r.AlcoholLicenseFileName) : null,
                        BusinessPermission = uploadService.GetPathForFileName(r.BusinessPermissionFileName),
                        IdCard = uploadService.GetPathForFileName(r.IdCardFileName),
                        Tables = r.Tables.Select(t => new TableVM
                        {
                            TableId = t.Id,
                            Capacity = t.Capacity
                        }),
                        ProvideDelivery = r.ProvideDelivery,
                        Logo = uploadService.GetPathForFileName(r.LogoFileName),
                        Photos = r.Photos.Select(p => uploadService.GetPathForFileName(p.PhotoFileName)).ToList(),
                        Description = r.Description,
                        ReservationDeposit = r.ReservationDeposit,
                        Tags = r.Tags.Select(t => t.Name).ToList(),
                        IsVerified = r.VerifierId is not null,
                        DistanceFrom = distance
                    });
                }
            }

            return nearRestaurants.OrderBy(r => r.DistanceFrom).ToList();
        }
        
        
        
        
        
        /// <summary>
        /// Register new Restaurant and optionally a new group for it.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<Result<RestaurantVM>> CreateRestaurantAsync(CreateRestaurantRequest request, User user)
        {
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
                    return new ValidationFailure
                    {
                        PropertyName = nameof(request.GroupId),
                        ErrorMessage = $"Group with ID {request.GroupId} not found",
                        ErrorCode = ErrorCodes.NotFound
                    };
                }

                if (group.OwnerId != user.Id)
                {
                    return new ValidationFailure
                    {
                        PropertyName = nameof(request.GroupId),
                        ErrorMessage = $"Group with ID {request.GroupId} is not owned by the current user",
                        ErrorCode = ErrorCodes.AccessDenied
                    };
                }
            }

            var result = await validationService.ValidateAsync(request, user.Id);
            if (!result.IsValid)
            {
                return result;
            }

            var restaurant = new Restaurant
            {
                Name = request.Name.Trim(),
                RestaurantType = request.RestaurantType,
                Address = request.Address.Trim(),
                Nip = request.Nip,
                PostalIndex = request.PostalIndex,
                City = request.City.Trim(),
                Location = new Point(request.Location.Longitude,request.Location.Latitude),
                Group = group,
                RentalContractFileName = request.RentalContract,
                AlcoholLicenseFileName = request.AlcoholLicense,
                BusinessPermissionFileName = request.BusinessPermission,
                IdCardFileName = request.IdCard,
                LogoFileName = request.Logo,
                ProvideDelivery = request.ProvideDelivery,
                Description = request.Description?.Trim(),
                ReservationDeposit = request.ReservationDeposit,
                Tags = await context.RestaurantTags
                    .Join(
                        request.Tags,
                        rt => rt.Name,
                        tag => tag,
                        (rt, _) => rt)
                    .ToListAsync(),
                Photos = request.Photos
                    .Select((photo, index) => new RestaurantPhoto
                    {
                        PhotoFileName = photo,
                        Order = index + 1
                    })
                    .ToList()
            };

            result = await validationService.ValidateAsync(restaurant, user.Id);
            if (!result.IsValid)
            {
                return result;
            }

            context.Add(restaurant);

            await context.SaveChangesAsync();

            return new RestaurantVM
            {
                RestaurantId = restaurant.Id,
                Name = restaurant.Name,
                RestaurantType = restaurant.RestaurantType,
                Nip = restaurant.Nip,
                Address = restaurant.Address,
                PostalIndex = restaurant.PostalIndex,
                City = restaurant.City,
                GroupId = restaurant.GroupId,
                GroupName = group.Name,
                RentalContract = restaurant.RentalContractFileName is not null ? uploadService.GetPathForFileName(restaurant.RentalContractFileName) : null,
                AlcoholLicense = restaurant.AlcoholLicenseFileName is not null ? uploadService.GetPathForFileName(restaurant.AlcoholLicenseFileName) : null,
                BusinessPermission = uploadService.GetPathForFileName(restaurant.BusinessPermissionFileName),
                IdCard = uploadService.GetPathForFileName(restaurant.IdCardFileName),
                Tables = [], //restaurantVM ma required pole Tables, ale nie dodajemy Tables powyżej
                ProvideDelivery = restaurant.ProvideDelivery,
                Logo = uploadService.GetPathForFileName(restaurant.LogoFileName),
                Photos = restaurant.Photos.Select(p => uploadService.GetPathForFileName(p.PhotoFileName)).ToList(),
                Description = restaurant.Description,
                Tags = restaurant.Tags.Select(t => t.Name).ToList(),
                IsVerified = restaurant.VerifierId is not null,
                Location = new Geolocation
                {
                    Longitude = restaurant.Location.X,
                    Latitude = restaurant.Location.Y
                },
                ReservationDeposit = restaurant.ReservationDeposit
            };
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
                .Where(r => r.Group.OwnerId == userId)
                .Select(r => new RestaurantSummaryVM
                {
                    RestaurantId = r.Id,
                    Name = r.Name,
                    Nip = r.Nip,
                    RestaurantType = r.RestaurantType,
                    Address = r.Address,
                    City = r.City,
                    Location = new Geolocation()
                    {
                        Longitude = r.Location.X,
                        Latitude = r.Location.Y
                    },
                    GroupId = r.GroupId,
                    ProvideDelivery = r.ProvideDelivery,
                    Logo = uploadService.GetPathForFileName(r.LogoFileName),
                    Description = r.Description,
                    ReservationDeposit = r.ReservationDeposit,
                    Tags = r.Tags.Select(t => t.Name).ToList(),
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
                .Where(r => r.Group.OwnerId == userId)
                .Where(r => r.Id == id)
                .Select(r => new RestaurantVM
                {
                    RestaurantId = r.Id,
                    Name = r.Name,
                    RestaurantType = r.RestaurantType,
                    Nip = r.Nip,
                    Address = r.Address,
                    PostalIndex = r.PostalIndex,
                    City = r.City,
                    Location = new Geolocation()
                    {
                        Longitude = r.Location.X,
                        Latitude = r.Location.Y
                    },
                    GroupId = r.Group.Id,
                    GroupName = r.Group.Name,
                    RentalContract = r.RentalContractFileName == null
                        ? null
                        : uploadService.GetPathForFileName(r.RentalContractFileName),
                    AlcoholLicense = r.AlcoholLicenseFileName == null
                        ? null
                        : uploadService.GetPathForFileName(r.AlcoholLicenseFileName),
                    BusinessPermission = uploadService.GetPathForFileName(r.BusinessPermissionFileName),
                    IdCard = uploadService.GetPathForFileName(r.IdCardFileName),
                    Tables = r.Tables.Select(t => new TableVM
                    {
                        TableId = t.Id,
                        Capacity = t.Capacity
                    }),
                    Photos = r.Photos
                        .OrderBy(rp => rp.Order)
                        .Select(rp => uploadService.GetPathForFileName(rp.PhotoFileName))
                        .ToList(),
                    ProvideDelivery = r.ProvideDelivery,
                    Logo = uploadService.GetPathForFileName(r.LogoFileName),
                    Description = r.Description,
                    ReservationDeposit = r.ReservationDeposit,
                    Tags = r.Tags.Select(t => t.Name).ToList(),
                    IsVerified = r.VerifierId != null
                })
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            return result;
        }

        /// <summary>
        /// Add employees to the given restaurant, acting as the given employer (restaurant owner)
        /// </summary>
        /// <param name="listRequest">Information about the employees to add</param>
        /// <param name="restaurantId">ID of the restaurant to add the employee to</param>
        /// <param name="employerId">ID of the current user (restaurant owner)</param>
        /// <returns>The bool returned inside the result does not mean anything</returns>
        public async Task<Result<bool>> AddEmployeeAsync(List<AddEmployeeRequest> listRequest, int restaurantId,
            string employerId)
        {
            var restaurantOwnerId = await context.Restaurants
                .Where(r => r.Id == restaurantId)
                .Select(r => r.Group.OwnerId)
                .FirstOrDefaultAsync();


            if (restaurantOwnerId is null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            foreach (var request in listRequest)
            {
                var result = await validationService.ValidateAsync(request, employerId);
                if (!result.IsValid)
                {
                    return result;
                }

                if (restaurantOwnerId != employerId)
                {
                    return new ValidationFailure
                    {
                        PropertyName = null,
                        ErrorCode = ErrorCodes.AccessDenied
                    };
                }

                var employee = await context.Users.FindAsync(request.EmployeeId);
                if (employee is null)
                {
                    return new ValidationFailure
                    {
                        PropertyName = nameof(request.EmployeeId),
                        ErrorCode = ErrorCodes.NotFound
                    };
                }

                if (!await userManager.IsInRoleAsync(employee, Roles.RestaurantEmployee)
                    || employee.EmployerId != employerId)
                {
                    return new ValidationFailure
                    {
                        PropertyName = nameof(request.EmployeeId),
                        ErrorCode = ErrorCodes.AccessDenied
                    };
                }

                var currentEmployment = await context.Employments
                    .Where(e => e.EmployeeId == request.EmployeeId && e.RestaurantId == restaurantId && e.DateUntil == null)
                    .FirstOrDefaultAsync();

                if (currentEmployment != null)
                {
                    return new ValidationFailure
                    {
                        PropertyName = nameof(request.EmployeeId), // zwracane jest Id pracownika, jako wskaźnik gdzie jest błąd
                        ErrorCode = ErrorCodes.EmployeeAlreadyEmployed
                    };
                }
            }

            await context.Employments.AddRangeAsync(listRequest.Select(r => new Employment
            {
                EmployeeId = r.EmployeeId,
                RestaurantId = restaurantId,
                IsBackdoorEmployee = r.IsBackdoorEmployee,
                IsHallEmployee = r.IsHallEmployee,
                DateFrom = DateOnly.FromDateTime(DateTime.Now)
            }).Select(x =>
            {
                Console.WriteLine(x.Id);
                return x;
            }));
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Move a restaurant to another group
        /// </summary>
        /// <param name="restaurantId">ID of the restaurant</param>
        /// <param name="request">Request details</param>
        /// <param name="user">Currently logged-in user</param>
        public async Task<Result<RestaurantSummaryVM>> MoveRestaurantToGroupAsync(int restaurantId,
            MoveToGroupRequest request, User user)
        {
            var newRestaurantGroup = await context.RestaurantGroups.Include(rg => rg.Restaurants)
                .FirstOrDefaultAsync(rg => rg.Id == request.GroupId && rg.OwnerId == user.Id);
            if (newRestaurantGroup == null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(request.GroupId),
                    ErrorMessage = $"RestaurantGroup with ID {request.GroupId} not found.",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var restaurant = await context.Restaurants
                .Include(r => r.Tags)
                .Include(r => r.Group)
                .ThenInclude(g => g.Restaurants)
                .FirstOrDefaultAsync(r => r.Id == restaurantId && r.Group.OwnerId == user.Id);
            if (restaurant == null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = $"Restaurant with ID {restaurantId} not found.",
                    ErrorCode = ErrorCodes.NotFound
                };
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
            context.RestaurantGroups.Update(newRestaurantGroup);
            await context.SaveChangesAsync();
            return new RestaurantSummaryVM
            {
                RestaurantId = restaurant.Id,
                Name = restaurant.Name,
                Nip = restaurant.Nip,
                RestaurantType = restaurant.RestaurantType,
                Address = restaurant.Address,
                City = restaurant.City,
                Location = new Geolocation()
                {
                    Longitude = restaurant.Location.X,
                    Latitude = restaurant.Location.Y
                },
                GroupId = restaurant.GroupId,
                Description = restaurant.Description,
                ReservationDeposit = restaurant.ReservationDeposit,
                Logo = uploadService.GetPathForFileName(restaurant.LogoFileName),
                Tags = restaurant.Tags.Select(t => t.Name).ToList(),
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
                .Include(r => r.Employments)
                .ThenInclude(e => e.Employee)
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();

            if (restaurant == null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = $"Restaurant with ID {id} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            if (restaurant.Group.OwnerId != userId)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = $"Restaurant with ID {id} is not owned by the current user",
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }

            return restaurant.Employments
                .Where(e => e.DateUntil == null)
                .Select(e => new RestaurantEmployeeVM
                {
                    EmploymentId = e.Id,
                    EmployeeId = e.EmployeeId,
                    Login = e.Employee.UserName!,
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
        public async Task<Result<RestaurantVM>> UpdateRestaurantAsync(int id, UpdateRestaurantRequest request,
            User user)
        {
            var restaurant = await context.Restaurants
                .Include(restaurant => restaurant.Group)
                .Include(restaurant => restaurant.Tables)
                .Include(restaurant => restaurant.Photos)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
            {
                return new ValidationFailure
                {
                    ErrorMessage = $"Restaurant with ID {id} not found.",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            if (restaurant.Group.OwnerId != user.Id)
            {
                return new ValidationFailure
                {
                    ErrorMessage = "User is not the owner of this restaurant.",
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }

            var result = await validationService.ValidateAsync(request, user.Id);
            if (!result.IsValid)
            {
                return result;
            }

            restaurant.Name = request.Name;
            restaurant.Nip = request.Nip;
            restaurant.RestaurantType = request.RestaurantType;
            restaurant.Address = request.Address;
            restaurant.PostalIndex = request.PostalIndex;
            restaurant.City = request.City;
            restaurant.ProvideDelivery = request.ProvideDelivery;
            restaurant.Description = request.Description;
            restaurant.ReservationDeposit = request.ReservationDeposit;

            restaurant.RentalContractFileName = request.RentalContract;
            restaurant.AlcoholLicenseFileName = request.AlcoholLicense;
            restaurant.BusinessPermissionFileName = request.BusinessPermission;
            restaurant.IdCardFileName = request.IdCard;
            restaurant.LogoFileName = request.Logo;


            restaurant.Tags = await context.RestaurantTags
                .Where(t => request.Tags.Contains(t.Name))
                .ToListAsync();


            // Update photos
            var photos = request.Photos
                .Select((photo, index) => new RestaurantPhoto
                {
                    PhotoFileName = photo,
                    Order = index + 1
                });

            restaurant.Photos.Clear();
            foreach (var photo in photos)
            {
                restaurant.Photos.Add(photo);
            }

            result = await validationService.ValidateAsync(restaurant, user.Id);
            if (!result.IsValid)
            {
                return result;
            }

            await context.SaveChangesAsync();

            return new RestaurantVM
            {
                RestaurantId = restaurant.Id,
                Name = restaurant.Name,
                RestaurantType = restaurant.RestaurantType,
                Nip = restaurant.Nip,
                Address = restaurant.Address,
                PostalIndex = restaurant.PostalIndex,
                City = restaurant.City,
                Location = new Geolocation()
                {
                    Longitude = restaurant.Location.X,
                    Latitude = restaurant.Location.Y
                },
                GroupId = restaurant.Group.Id,
                GroupName = restaurant.Group.Name,
                RentalContract = restaurant.RentalContractFileName == null
                    ? null
                    : uploadService.GetPathForFileName(restaurant.RentalContractFileName),
                AlcoholLicense = restaurant.AlcoholLicenseFileName == null
                    ? null
                    : uploadService.GetPathForFileName(restaurant.AlcoholLicenseFileName),
                BusinessPermission = uploadService.GetPathForFileName(restaurant.BusinessPermissionFileName),
                IdCard = uploadService.GetPathForFileName(restaurant.IdCardFileName),
                Tables = restaurant.Tables.Select(t => new TableVM
                {
                    TableId = t.Id,
                    Capacity = t.Capacity
                }),
                Photos = restaurant.Photos
                    .OrderBy(rp => rp.Order)
                    .Select(rp => uploadService.GetPathForFileName(rp.PhotoFileName))
                    .ToList(),
                ProvideDelivery = restaurant.ProvideDelivery,
                Logo = uploadService.GetPathForFileName(restaurant.LogoFileName),
                Description = restaurant.Description,
                ReservationDeposit = restaurant.ReservationDeposit,
                Tags = restaurant.Tags.Select(t => t.Name).ToList(),
                IsVerified = restaurant.VerifierId != null
            };
        }


        /// <summary>
        /// Returns a specific restaurant owned by the user.
        /// </summary>
        /// <param name="user">Currently logged-in user</param>
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
            var result = await validationService.ValidateAsync(dto, user.Id);
            if (!result.IsValid)
            {
                return result;
            }

            if (dto.GroupId != null)
            {
                var group = await context.RestaurantGroups.FindAsync(dto.GroupId);

                if (group is null)
                {
                    return new ValidationFailure
                    {
                        PropertyName = nameof(dto.GroupId),
                        ErrorMessage = $"Group with ID {dto.GroupId} not found",
                        ErrorCode = ErrorCodes.NotFound
                    };
                }

                if (group.OwnerId != user.Id)
                {
                    return new ValidationFailure
                    {
                        PropertyName = nameof(dto.GroupId),
                        ErrorMessage = $"Group with ID {dto.GroupId} is not owned by the current user",
                        ErrorCode = ErrorCodes.AccessDenied
                    };
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a list of menus of specific restaurant
        /// </summary>
        /// <param name="id"> Id of the restaurant.</param>
        /// <returns></returns>
        public async Task<List<MenuSummaryVM>?> GetMenusAsync(int id)
        {
            var restaurant = await context.Restaurants
                .Include(r => r.Menus)
                .Where(i => i.Id == id)
                .FirstOrDefaultAsync();

            if (restaurant == null)
            {
                return null;
            }

            var menus = restaurant.Menus
                .Select(menu => new MenuSummaryVM
                {
                    MenuId = menu.Id,
                    Name = menu.Name,
                    AlternateName = menu.AlternateName,
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
            var isRestaurantValid = await menuItemsService.ValidateRestaurant(user, restaurantId);

            if (isRestaurantValid.IsError)
            {
                return isRestaurantValid.Errors;
            }

            return await context.MenuItems
                .Where(i => i.RestaurantId == restaurantId)
                .Select(i => new MenuItemVM()
                {
                    MenuItemId = i.Id,
                    Name = i.Name,
                    AlternateName = i.AlternateName,
                    Price = i.Price,
                    AlcoholPercentage = i.AlcoholPercentage,
                    Photo = uploadService.GetPathForFileName(i.PhotoFileName)
                }).ToListAsync();
        }

        /// <summary>
        /// Function for soft deleting Restaurants that also deletes newly emptied restaurant groups
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<Result<bool>> SoftDeleteRestaurantAsync(int id, User user)
        {
            var restaurant = await context.Restaurants
                .AsSplitQuery()
                .Include(r => r.Group)
                .ThenInclude(g => g.Restaurants)
                .Include(restaurant => restaurant.Tables)
                .Include(restaurant => restaurant.Employments)
                .Include(restaurant => restaurant.Photos)
                .Include(restaurant => restaurant.Menus)
                .Include(restaurant => restaurant.MenuItems)
                .Where(r => r.Id == id && r.Group.OwnerId == user.Id)
                .FirstOrDefaultAsync();
            if (restaurant == null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorCode = ErrorCodes.NotFound,
                    ErrorMessage = ErrorCodes.NotFound
                };
            }

            context.RemoveRange(restaurant.Tables);
            context.RemoveRange(restaurant.Employments);
            context.RemoveRange(restaurant.Photos);
            context.RemoveRange(restaurant.MenuItems);
            context.RemoveRange(restaurant.Menus);

            context.Remove(restaurant);
            if (restaurant.Group.Restaurants.Count == 0)
            {
                context.Remove(restaurant.Group);
            }

            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Get orders in a restaurant
        /// </summary>
        /// <param name="userId">Currently logged-in user, must be an employee in the restaurant</param>
        /// <param name="restaurantId">ID of the restaurant</param>
        /// <param name="returnFinished">Return finished orders, return current orders if false</param>
        /// <param name="page">Page to return</param>
        /// <param name="perPage">Items per page</param>
        /// <param name="orderBy">Sorting order</param>
        /// <returns>Paginated order list</returns>
        public async Task<Result<Pagination<OrderSummaryVM>>> GetOrdersAsync(string userId, int restaurantId,
            bool returnFinished = false, int page = 0, int perPage = 10, OrderSorting? orderBy = null)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(userId),
                    ErrorMessage = $"User with ID {userId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            if (!await userManager.IsInRoleAsync(user, Roles.RestaurantEmployee))
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(userId),
                    ErrorMessage = $"User with ID {userId} is not a RestaurantEmployee",
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }

            var isEmployeeAtRestaurant = await context.Employments.AnyAsync(e =>
                e.EmployeeId == userId && e.RestaurantId == restaurantId && e.DateUntil == null);
            if (!isEmployeeAtRestaurant)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(userId),
                    ErrorMessage = $"User with ID {userId} is not employed at restaurant with ID {restaurantId}",
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }

            var restaurant = await context.Restaurants.FindAsync(restaurantId);
            if (restaurant == null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(restaurantId),
                    ErrorMessage = $"Restaurant with ID {restaurantId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var ordersQuery = context.Orders
                .Include(order => order.Visit)
                .Include(order => order.OrderItems)
                .ThenInclude(orderItem => orderItem.MenuItem)
                .Where(order => order.Visit.TableRestaurantId == restaurantId);

            if (returnFinished)
            {
                ordersQuery = ordersQuery.Where(order =>
                    order.OrderItems.Max(oi => oi.Status) == OrderStatus.Taken ||
                    order.OrderItems.Max(oi => oi.Status) == OrderStatus.Cancelled);
            }
            else
            {
                ordersQuery = ordersQuery.Where(order =>
                    order.OrderItems.Max(oi => oi.Status) == OrderStatus.InProgress ||
                    order.OrderItems.Max(oi => oi.Status) == OrderStatus.Ready);
            }

            var filteredOrders = ordersQuery.Select(order => new OrderSummaryVM
            {
                OrderId = order.Id,
                VisitId = order.VisitId,
                Date = order.Visit.Date,
                Note = order.Note,
                Cost = order.OrderItems.Sum(oi => oi.MenuItem.Price * oi.Amount),
                Status = order.OrderItems.Max(oi => oi.Status)
            });

            filteredOrders = orderBy switch
            {
                OrderSorting.CostAsc => filteredOrders.OrderBy(o => o.Cost),
                OrderSorting.CostDesc => filteredOrders.OrderByDescending(o => o.Cost),
                OrderSorting.DateAsc => filteredOrders.OrderBy(o => o.Date),
                OrderSorting.DateDesc => filteredOrders.OrderByDescending(o => o.Date),
                _ => filteredOrders
            };

            return await filteredOrders.PaginateAsync(page, perPage);
        }
    }
}
