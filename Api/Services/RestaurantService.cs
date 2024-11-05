using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;
using Reservant.Api.Identity;
using Reservant.Api.Models.Enums;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Deliveries;
using Reservant.Api.Dtos.Events;
using Reservant.Api.Dtos.Ingredients;
using Reservant.Api.Dtos.Menus;
using Reservant.Api.Dtos.MenuItems;
using Reservant.Api.Dtos.Orders;
using Reservant.Api.Dtos.Restaurants;
using Reservant.Api.Dtos.Reviews;
using Reservant.Api.Dtos.Visits;
using Reservant.Api.Dtos.Location;
using Reservant.Api.Dtos.Users;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Reservant.Api.Mapping;
using Microsoft.AspNetCore.Authorization;

namespace Reservant.Api.Services
{
    /// <summary>
    /// Service responsible for managing restaurants
    /// </summary>
    public class RestaurantService(
        ApiDbContext context,
        UrlService urlService,
        UserManager<User> userManager,
        ValidationService validationService,
        GeometryFactory geometryFactory,
        AuthorizationService authorizationService,
        NotificationService notificationService,
        IMapper mapper)
    {
        /// <summary>
        /// Find restaurants by different criteria
        /// </summary>
        /// <remarks>
        /// Returns them sorted from the nearest to the farthest if origLat and origLon are provided;
        /// Else sorts them alphabetically by name
        /// </remarks>
        /// <param name="origLat">Latitude of the point to search from; if provided the restaurants will be sorted by distance</param>
        /// <param name="origLon">Longitude of the point to search from; if provided the restaurants will be sorted by distance</param>
        /// <param name="name">Search by name</param>
        /// <param name="tags">Search restaurants that have certain tags (up to 4)</param>
        /// <param name="minRating">Search restaurants with at least this many stars</param>
        /// <param name="page">Page number</param>
        /// <param name="perPage">Items per page</param>
        /// <param name="lat1">Search within a rectengular area: first point's latitude</param>
        /// <param name="lon1">Search within a rectengular area: first point's longitude</param>
        /// <param name="lat2">Search within a rectengular area: second point's latitude</param>
        /// <param name="lon2">Search within a rectengular area: second point's longitude</param>
        /// <returns></returns>
        public async Task<Result<Pagination<NearRestaurantVM>>> FindRestaurantsAsync(
            double? origLat, double? origLon,
            string? name, HashSet<string> tags, int? minRating,
            double? lat1, double? lon1, double? lat2, double? lon2,
            int page, int perPage)
        {
            IQueryable<Restaurant> query = context.Restaurants
                .AsNoTracking()
                .OnlyActiveRestaurants();

            if (name is not null)
            {
                query = query.Where(r => r.Name.Contains(name.Trim()));
            }

            if (tags.Count > 0)
            {
                if (tags.Count > 4)
                {
                    return new ValidationFailure
                    {
                        PropertyName = nameof(tags),
                        ErrorMessage = "Only up to 4 tags can be specified",
                        ErrorCode = ErrorCodes.InvalidSearchParameters,
                    };
                }

                foreach (var tag in tags)
                {
                    query = query.Where(r => r.Tags.Any(t => t.Name == tag));
                }
            }

            if (minRating is not null)
            {
                if (minRating < 0 || minRating > 5)
                {
                    return new ValidationFailure
                    {
                        PropertyName = nameof(minRating),
                        ErrorMessage = "Minimum rating must be from 0 to 5",
                        ErrorCode = ErrorCodes.InvalidSearchParameters,
                    };
                }

                query = query.Where(r => r.Reviews.Average(review => (double?)review.Stars) >= minRating);
            }

            if (lat1 is not null || lon1 is not null || lat2 is not null || lon2 is not null)
            {
                if (lat1 is null || lon1 is null || lat2 is null || lon2 is null)
                {
                    return new ValidationFailure
                    {
                        PropertyName = null,
                        ErrorMessage = "To search within a rectangular area, all 4 coordinates " +
                                       $"must be provided: {nameof(lat1)}, {nameof(lon1)}, {nameof(lat2)}, {nameof(lon2)}",
                        ErrorCode = ErrorCodes.InvalidSearchParameters
                    };
                }

                var minLat = Math.Min(lat1.Value, lat2.Value);
                var maxLat = Math.Max(lat1.Value, lat2.Value);
                var minLon = Math.Min(lon1.Value, lon2.Value);
                var maxLon = Math.Max(lon1.Value, lon2.Value);

                var coordinates = new[]
                {
                    new Coordinate(minLon, minLat),
                    new Coordinate(maxLon, minLat),
                    new Coordinate(maxLon, maxLat),
                    new Coordinate(minLon, maxLat),
                    new Coordinate(minLon, minLat)
                };
                var boundingBox = geometryFactory.CreatePolygon(coordinates);

                if (!boundingBox.IsValid)
                {
                    return new ValidationFailure
                    {
                        PropertyName = nameof(boundingBox),
                        ErrorMessage = "Given coordinates does not create a valid Polygon!",
                        ErrorCode = ErrorCodes.InvalidSearchParameters
                    };
                }

                query = query.Where(r => boundingBox.Contains(r.Location));
            }

            var origin = geometryFactory.CreatePoint();

            if (origLat is not null || origLon is not null)
            {
                if (origLat is null || origLon is null)
                {
                    return new ValidationFailure
                    {
                        PropertyName = null,
                        ErrorMessage = $"To search starting from a point both {nameof(origLat)} {nameof(origLon)}",
                        ErrorCode = ErrorCodes.InvalidSearchParameters,
                    };
                }

                origin = geometryFactory.CreatePoint(new Coordinate(origLon!.Value, origLat!.Value));
                query = query.OrderBy(r => origin.Distance(r.Location));
            }
            else
            {
                query = query.OrderBy(r => r.Name);
            }

            var nearRestaurants = await query
                .Select(r => new NearRestaurantVM
                {
                    RestaurantId = r.RestaurantId,
                    Name = r.Name,
                    RestaurantType = r.RestaurantType,
                    Address = r.Address,
                    City = r.City,
                    Location = new Geolocation
                    {
                        Latitude = r.Location.Y,
                        Longitude = r.Location.X
                    },
                    ProvideDelivery = r.ProvideDelivery,
                    Logo = urlService.GetPathForFileName(r.LogoFileName),
                    Description = r.Description,
                    ReservationDeposit = r.ReservationDeposit,
                    Tags = r.Tags.Select(t => t.Name).ToList(),
                    DistanceFrom = origin.IsEmpty ? null : origin.Distance(r.Location),
                    Rating = r.Reviews.Select(rv => (double?)rv.Stars).Average() ?? 0,
                    NumberReviews = r.Reviews.Count,
                    OpeningHours = r.OpeningHours.ToList(),
                })
                .PaginateAsync(page, perPage, []);

            return nearRestaurants;
        }

        /// <summary>
        /// Register new Restaurant and optionally a new group for it.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [ErrorCode(nameof(CreateRestaurantRequest.GroupId), ErrorCodes.NotFound)]
        [ErrorCode(nameof(CreateRestaurantRequest.GroupId), ErrorCodes.AccessDenied,
            "Group with ID is not owned by the current user")]
        [ValidatorErrorCodes<CreateRestaurantRequest>]
        [ValidatorErrorCodes<Restaurant>]
        public async Task<Result<MyRestaurantVM>> CreateRestaurantAsync(CreateRestaurantRequest request, User user)
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
                Location = geometryFactory.CreatePoint(new Coordinate(request.Location.Longitude,
                    request.Location.Latitude)),
                Group = group,
                RentalContractFileName = request.RentalContract,
                AlcoholLicenseFileName = request.AlcoholLicense,
                BusinessPermissionFileName = request.BusinessPermission,
                IdCardFileName = request.IdCard,
                MaxReservationDurationMinutes = request.MaxReservationDurationMinutes,
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
                    .ToList(),
                OpeningHours = new WeeklyOpeningHours(request.OpeningHours),
            };

            result = await validationService.ValidateAsync(restaurant, user.Id);
            if (!result.IsValid)
            {
                return result;
            }

            context.Add(restaurant);

            await context.SaveChangesAsync();

            return mapper.Map<MyRestaurantVM>(restaurant);
        }

        /// <summary>
        /// Returns a list of restaurants owned by the user.
        /// </summary>
        /// <param name="name">Search by name</param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<List<RestaurantSummaryVM>> GetMyRestaurantsAsync(User user, string? name = null)
        {
            var userId = user.Id;
            var result = await context.Restaurants
                .AsNoTracking()
                .Where(r => r.Group.OwnerId == userId)
                .Where(r => name == null || r.Name.Contains(name.Trim()))
                .ProjectTo<RestaurantSummaryVM>(mapper.ConfigurationProvider)
                .ToListAsync();
            return result;
        }

        /// <summary>
        /// Returns a specific restaurant owned by the user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="id"> Id of the restaurant.</param>
        /// <returns></returns>
        public async Task<MyRestaurantVM?> GetMyRestaurantByIdAsync(User user, int id)
        {
            var userId = user.Id;
            var result = await context.Restaurants
                .AsNoTracking()
                .Where(r => r.Group.OwnerId == userId)
                .Where(r => r.RestaurantId == id)
                .ProjectTo<MyRestaurantVM>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            return result;
        }

        /// <summary>
        /// Add employees to the given restaurant, acting as the given employer (restaurant owner)
        /// </summary>
        /// <param name="listRequest">Information about the employees to add</param>
        /// <param name="restaurantId">ID of the restaurant to add the employee to</param>
        /// <param name="employerId">ID of the current user (restaurant owner)</param>
        [ErrorCode(null, ErrorCodes.NotFound)]
        [ValidatorErrorCodes<AddEmployeeRequest>]
        [ErrorCode(null, ErrorCodes.AccessDenied, "Restaurant not owned by user")]
        [ErrorCode(nameof(AddEmployeeRequest.EmployeeId), ErrorCodes.NotFound)]
        [ErrorCode(nameof(AddEmployeeRequest.EmployeeId), ErrorCodes.AccessDenied,
            "User is not a restaurant employee or is not employee of the restaurant owner")]
        [ErrorCode(nameof(AddEmployeeRequest.EmployeeId), ErrorCodes.EmployeeAlreadyEmployed,
            "Employee is alredy employed in a restaurant")]
        [ErrorCode(nameof(AddEmployeeRequest.EmployeeId), ErrorCodes.MustBeCurrentUsersEmployee)]
        public async Task<Result> AddEmployeeAsync(List<AddEmployeeRequest> listRequest, int restaurantId,
            Guid employerId)
        {
            var restaurantOwnerId = await context.Restaurants
                .Where(r => r.RestaurantId == restaurantId)
                .Select(r => r.Group.OwnerId)
                .FirstOrDefaultAsync();


            if (restaurantOwnerId == Guid.Empty)
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

                if (employee.EmployerId != employerId)
                {
                    return new ValidationFailure
                    {
                        PropertyName = nameof(request.EmployeeId),
                        ErrorCode = ErrorCodes.MustBeCurrentUsersEmployee,
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
                    .Where(e => e.EmployeeId == request.EmployeeId && e.RestaurantId == restaurantId &&
                                e.DateUntil == null)
                    .FirstOrDefaultAsync();

                if (currentEmployment != null)
                {
                    return new ValidationFailure
                    {
                        PropertyName =
                            nameof(request.EmployeeId), // zwracane jest Id pracownika, jako wskaźnik gdzie jest błąd
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
                DateFrom = DateOnly.FromDateTime(DateTime.UtcNow)
            }));
            await context.SaveChangesAsync();
            return Result.Success;
        }

        /// <summary>
        /// Move a restaurant to another group
        /// </summary>
        /// <param name="restaurantId">ID of the restaurant</param>
        /// <param name="request">Request details</param>
        /// <param name="user">Currently logged-in user</param>
        [ErrorCode(nameof(MoveToGroupRequest.GroupId), ErrorCodes.NotFound)]
        [ErrorCode(null, ErrorCodes.NotFound)]
        public async Task<Result<RestaurantSummaryVM>> MoveRestaurantToGroupAsync(int restaurantId,
            MoveToGroupRequest request, User user)
        {
            var newRestaurantGroup = await context.RestaurantGroups.Include(rg => rg.Restaurants)
                .FirstOrDefaultAsync(rg => rg.RestaurantGroupId == request.GroupId && rg.OwnerId == user.Id);
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
                .AsSplitQuery()
                .Include(r => r.Tags)
                .Include(r => r.Group)
                .ThenInclude(g => g.Restaurants)
                .Include(r => r.Reviews)
                .FirstOrDefaultAsync(r => r.RestaurantId == restaurantId && r.Group.OwnerId == user.Id);
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
                oldGroup.IsDeleted = true;
            }

            restaurant.GroupId = request.GroupId;
            restaurant.Group = newRestaurantGroup;
            newRestaurantGroup.Restaurants.Add(restaurant);
            context.RestaurantGroups.Update(newRestaurantGroup);
            await context.SaveChangesAsync();
            return mapper.Map<RestaurantSummaryVM>(restaurant);
        }

        /// <summary>
        /// Get list of restaurant's current employees
        /// </summary>
        /// <param name="id">ID of the restaurants</param>
        /// <param name="userId">ID of the current user (to check permissions)</param>
        [ErrorCode(null, ErrorCodes.NotFound)]
        [ErrorCode(null, ErrorCodes.AccessDenied, "Restaurant with ID is not owned by the current user")]
        public async Task<Result<List<RestaurantEmployeeVM>>> GetEmployeesAsync(int id, Guid userId)
        {
            var restaurant = await context.Restaurants
                .AsNoTracking()
                .Include(r => r.Group)
                .Include(r => r.Employments)
                .ThenInclude(e => e.Employee)
                .Where(r => r.RestaurantId == id)
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
                    EmploymentId = e.EmploymentId,
                    EmployeeId = e.EmployeeId,
                    Login = e.Employee.UserName!,
                    FirstName = e.Employee.FirstName,
                    LastName = e.Employee.LastName,
                    BirthDate = e.Employee.BirthDate!.Value,
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
        [ErrorCode(null, ErrorCodes.NotFound)]
        [ErrorCode(null, ErrorCodes.AccessDenied, "User is not the owner of this restaurant.")]
        [ValidatorErrorCodes<UpdateRestaurantRequest>]
        [ValidatorErrorCodes<Restaurant>]
        public async Task<Result<MyRestaurantVM>> UpdateRestaurantAsync(int id, UpdateRestaurantRequest request,
            User user)
        {
            var restaurant = await context.Restaurants
                .AsSplitQuery()
                .Include(restaurant => restaurant.Group)
                .Include(restaurant => restaurant.Tables)
                .Include(restaurant => restaurant.Photos)
                .Include(restaurant => restaurant.Tags)
                .FirstOrDefaultAsync(r => r.RestaurantId == id);

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

            restaurant.Name = request.Name.Trim();
            restaurant.Nip = request.Nip.Trim();
            restaurant.RestaurantType = request.RestaurantType;
            restaurant.Address = request.Address.Trim();
            restaurant.PostalIndex = request.PostalIndex.Trim();
            restaurant.City = request.City.Trim();
            restaurant.ProvideDelivery = request.ProvideDelivery;
            restaurant.Description = request.Description?.Trim();
            restaurant.ReservationDeposit = request.ReservationDeposit;
            restaurant.MaxReservationDurationMinutes = request.MaxReservationDurationMinutes;

            restaurant.RentalContractFileName = request.RentalContract;
            restaurant.AlcoholLicenseFileName = request.AlcoholLicense;
            restaurant.BusinessPermissionFileName = request.BusinessPermission;
            restaurant.IdCardFileName = request.IdCard;
            restaurant.LogoFileName = request.Logo;
            restaurant.Location = geometryFactory.CreatePoint(new Coordinate(request.Location.Longitude,
             request.Location.Latitude));

            restaurant.OpeningHours = new WeeklyOpeningHours(request.OpeningHours);

            restaurant.Tags = await context.RestaurantTags
                .Where(t => request.Tags.Contains(t.Name))
                .ToListAsync();

            var photos = request.Photos
                .Select((photo, index) => new RestaurantPhoto
                {
                    PhotoFileName = photo,
                    Order = index + 1
                }).ToList();

            restaurant.Photos = photos;

            result = await validationService.ValidateAsync(restaurant, user.Id);
            if (!result.IsValid)
            {
                return result;
            }

            await context.SaveChangesAsync();

            return mapper.Map<MyRestaurantVM>(restaurant);
        }

        /// <summary>
        /// Returns a specific restaurant owned by the user.
        /// </summary>
        /// <param name="userId">ID of the currently logged-in user</param>
        /// <param name="idRestaurant"> Id of the restaurant.</param>
        /// <returns></returns>
        [ErrorCode(null, ErrorCodes.NotFound, "Restaurant not found")]
        [ErrorCode(null, ErrorCodes.Duplicate, "Restaurant already verified")]
        public async Task<Result> SetVerifiedIdAsync(Guid userId, int idRestaurant)
        {
            var result = await context
                .Restaurants
                .Include(r => r.Group)
                .Where(r => r.RestaurantId == idRestaurant)
                .FirstOrDefaultAsync();

            if (result is null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = $"Restaurant with ID {idRestaurant} not found",
                    ErrorCode = ErrorCodes.NotFound,
                };
            }

            if (result.VerifierId is not null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = $"Restaurant with ID {idRestaurant} is already verified",
                    ErrorCode = ErrorCodes.Duplicate,
                };
            }

            result.VerifierId = userId;
            await context.SaveChangesAsync();

            await notificationService.NotifyRestaurantVerified(
                result.Group.OwnerId, idRestaurant);

            return Result.Success;
        }

        /// <summary>
        /// Validates if given dto is valid. If a group is given, checks if that group belongs to User
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [ErrorCode(nameof(ValidateRestaurantFirstStepRequest.GroupId), ErrorCodes.NotFound)]
        [ErrorCode(nameof(ValidateRestaurantFirstStepRequest.GroupId), ErrorCodes.AccessDenied,
            "Group with ID is not owned by the current user")]
        public async Task<Result> ValidateFirstStepAsync(ValidateRestaurantFirstStepRequest dto, User user)
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

            return Result.Success;
        }

        /// <summary>
        /// Returns a list of menus of a specific restaurant (owner version)
        /// </summary>
        /// <param name="restaurantId">Id of the restaurant.</param>
        /// <returns>A Result object containing a list of MenuSummaryVM or an error message.</returns>
        public async Task<Result<List<MenuSummaryVM>>> GetMenusCustomerAsync(int restaurantId)
        {
            var todaysDate = DateOnly.FromDateTime(DateTime.UtcNow);

            var restaurant = await context.Restaurants
                .AsNoTracking()
                .Include(r => r.Menus)
                .OnlyActiveRestaurants()
                .Where(i => i.RestaurantId == restaurantId)
                .FirstOrDefaultAsync();

            if (restaurant == null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(restaurantId),
                    ErrorMessage = $"Restaurant with ID {restaurantId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var menus = restaurant.Menus
                .Where(m => (m.DateUntil ?? todaysDate) >= todaysDate);

            return mapper.Map<List<MenuSummaryVM>>(menus);
        }


        /// <summary>
        /// Validates and gets menu items from the given restaurant
        /// </summary>
        /// <param name="user"></param>
        /// <param name="restaurantId"></param>
        /// <returns>MenuItems</returns>
        public async Task<Result<List<MenuItemVM>>> GetMenuItemsCustomerAsync(User user, int restaurantId)
        {
            var restaurant = await context.Restaurants
                .AsNoTracking()
                .Include(r => r.Menus)
                .OnlyActiveRestaurants()
                .Where(i => i.RestaurantId == restaurantId)
                .FirstOrDefaultAsync();

            if (restaurant == null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(restaurantId),
                    ErrorMessage = $"Restaurant with ID {restaurantId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            return await context.MenuItems
                .Where(i => i.RestaurantId == restaurantId)
                .ProjectTo<MenuItemVM>(mapper.ConfigurationProvider)
                .ToListAsync();
        }

        /// <summary>
        /// Function for soft deleting Restaurants that also deletes newly emptied restaurant groups
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [ErrorCode(null, ErrorCodes.NotFound)]
        public async Task<Result> ArchiveRestaurantAsync(int id, User user)
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
                .Where(r => r.RestaurantId == id && r.Group.OwnerId == user.Id)
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

            foreach (var table in restaurant.Tables)
            {
                table.IsDeleted = true;
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            foreach (var employment in restaurant.Employments)
            {
                employment.DateUntil = today;
            }

            foreach (var menuItem in restaurant.MenuItems)
            {
                menuItem.IsDeleted = true;
            }

            foreach (var menu in restaurant.Menus)
            {
                menu.IsDeleted = true;
            }

            restaurant.IsArchived = true;

            // We check if the restaurant was the last one (the collection was loaded before we deleted it)
            if (restaurant.Group.Restaurants.Count == 1)
            {
                restaurant.Group.IsDeleted = true;
            }

            await context.SaveChangesAsync();
            return Result.Success;
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
        /// <param name="tableId">Optional table number filter by Id</param>
        /// <param name="assignedEmployeeId">Optional emplyee number filter by Id</param>
        /// <returns>Paginated order list</returns>
        public async Task<Result<Pagination<OrderSummaryVM>>> GetOrdersAsync(Guid userId, int restaurantId,
            bool returnFinished = false, int page = 0, int perPage = 10, OrderSorting? orderBy = null, int? tableId = null, Guid? assignedEmployeeId = null)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(userId),
                    ErrorMessage = $"User with ID {userId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var restaurant = await context.Restaurants
                .AsNoTracking()
                .Include(restaurant => restaurant.Group)
                .FirstOrDefaultAsync(x => x.RestaurantId == restaurantId);
            if (restaurant == null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(restaurantId),
                    ErrorMessage = $"Restaurant with ID {restaurantId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var roles = await userManager.GetRolesAsync(user);

            if (roles.Contains(Roles.RestaurantEmployee))
            {
                var isEmployeeAtRestaurant = await context.Employments.AnyAsync(e =>
                    e.EmployeeId == userId && e.RestaurantId == restaurantId && e.DateUntil == null);
                if (!isEmployeeAtRestaurant)
                {
                    return new ValidationFailure
                    {
                        PropertyName = null,
                        ErrorMessage = $"User with ID {userId} is not employed at restaurant with ID {restaurantId}",
                        ErrorCode = ErrorCodes.AccessDenied
                    };
                }
            }
            else if (roles.Contains(Roles.RestaurantOwner))
            {
                if (restaurant.Group.OwnerId != userId)
                {
                    return new ValidationFailure
                    {
                        PropertyName = null,
                        ErrorMessage = "User is not owner of the restaurant",
                        ErrorCode = ErrorCodes.AccessDenied
                    };
                }
            }
            else
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "User must be employed in the restaurant or be its owner",
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }

            var ordersQuery = context.Orders
                .Include(order => order.Visit)
                .Include(order => order.OrderItems)
                .ThenInclude(orderItem => orderItem.MenuItem)
                .Where(order => order.Visit.RestaurantId == restaurantId);

            if (tableId != null)
            {
                ordersQuery = ordersQuery.Where(order => order.Visit.TableId == tableId);
            }

            if (assignedEmployeeId != null)
            {
                ordersQuery = ordersQuery.Where(order => order.AssignedEmployeeId == assignedEmployeeId);
            }


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

            var filteredOrders = ordersQuery.ProjectTo<OrderSummaryVM>(mapper.ConfigurationProvider);

            filteredOrders = orderBy switch
            {
                OrderSorting.CostAsc => filteredOrders.OrderBy(o => o.Cost),
                OrderSorting.CostDesc => filteredOrders.OrderByDescending(o => o.Cost),
                OrderSorting.DateAsc => filteredOrders.OrderBy(o => o.Date),
                OrderSorting.DateDesc => filteredOrders.OrderByDescending(o => o.Date),
                _ => filteredOrders
            };

            return await filteredOrders.PaginateAsync(page, perPage, Enum.GetNames<OrderSorting>());
        }

        /// <summary>
        /// Get future events in a restaurant with pagination.
        /// </summary>
        /// <param name="restaurantId">ID of the restaurant.</param>
        /// <param name="page">Page number to return.</param>
        /// <param name="perPage">Items per page.</param>
        /// <returns>Paginated list of future events.</returns>
        public async Task<Result<Pagination<EventSummaryVM>>> GetFutureEventsByRestaurantAsync(int restaurantId,
            int page, int perPage)
        {
            var restaurant = await context.Restaurants
                .AsNoTracking()
                .OnlyActiveRestaurants()
                .SingleOrDefaultAsync(restaurant => restaurant.RestaurantId == restaurantId);
            if (restaurant == null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(restaurantId),
                    ErrorMessage = $"Restaurant with ID {restaurantId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var query = context.Events
                .Where(e => e.RestaurantId == restaurantId && e.Time > DateTime.UtcNow)
                .OrderBy(e => e.Time)
                .ProjectTo<EventSummaryVM>(mapper.ConfigurationProvider);

            return await query.PaginateAsync(page, perPage, []);
        }

        /// <summary>
        /// Add review to restaurant of given id from logged in user containing data from request
        /// </summary>
        /// <param name="user">User putting in a review</param>
        /// <param name="restaurantId">ID of restaurant reciving review</param>
        /// <param name="createReviewRequest">template for data provided in a reveiw</param>
        /// <returns>View of a created review</returns>
        [ErrorCode(null, ErrorCodes.NotFound)]
        [ErrorCode(null, ErrorCodes.Duplicate, "If the user has already reviewed the restaurant")]
        [ValidatorErrorCodes<CreateReviewRequest>]
        [ValidatorErrorCodes<Review>]
        public async Task<Result<ReviewVM>> CreateReviewAsync(User user, int restaurantId,
            CreateReviewRequest createReviewRequest)
        {
            var restaurant = await context.Restaurants
                .Include(r => r.Group)
                .OnlyActiveRestaurants()
                .Where(r => r.RestaurantId == restaurantId)
                .FirstOrDefaultAsync();

            if (restaurant == null)
            {
                return new ValidationFailure { PropertyName = null, ErrorCode = ErrorCodes.NotFound };
            }

            var createReviewRequestValidation = await validationService.ValidateAsync(createReviewRequest, user.Id);
            if (!createReviewRequestValidation.IsValid)
            {
                return createReviewRequestValidation;
            }

            var existingReview = await context.Reviews
                .Where(r => r.RestaurantId == restaurantId)
                .Where(r => r.Author == user)
                .FirstOrDefaultAsync();

            if (existingReview != null)
            {
                return new ValidationFailure { PropertyName = null, ErrorCode = ErrorCodes.Duplicate };
            }

            var newReview = new Review
            {
                Restaurant = restaurant,
                Author = user,
                RestaurantId = restaurantId,
                AuthorId = user.Id,
                Stars = createReviewRequest.Stars,
                CreatedAt = DateTime.UtcNow,
                Contents = createReviewRequest.Contents
            };


            var reviewValidation = await validationService.ValidateAsync(newReview, user.Id);
            if (!reviewValidation.IsValid)
            {
                return reviewValidation;
            }

            await context.Reviews.AddAsync(newReview);
            await context.SaveChangesAsync();

            await notificationService.NotifyNewRestaurantReview(restaurant.Group.OwnerId, newReview.ReviewId);

            var reviewVM = new ReviewVM
            {
                ReviewId = newReview.ReviewId,
                RestaurantId = newReview.RestaurantId,
                AuthorId = newReview.AuthorId,
                AuthorFullName = newReview.Author.FullName,
                Stars = newReview.Stars,
                CreatedAt = newReview.CreatedAt,
                DateEdited = newReview.DateEdited,
                Contents = newReview.Contents,
                AnsweredAt = newReview.AnsweredAt,
                RestaurantResponse = newReview.RestaurantResponse
            };

            return reviewVM;
        }

        /// <summary>
        /// Get reviews for a restaurant
        /// </summary>
        [ErrorCode(null, ErrorCodes.NotFound)]
        public async Task<Result<Pagination<ReviewVM>>> GetReviewsAsync(int restaurantId,
            ReviewOrderSorting orderBy = ReviewOrderSorting.DateDesc, int page = 0, int perPage = 10)
        {
            var restaurant = await context.Restaurants
                .AsNoTracking()
                .OnlyActiveRestaurants()
                .SingleOrDefaultAsync(restaurant => restaurant.RestaurantId == restaurantId);

            if (restaurant == null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = $"Restaurant with ID {restaurantId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var reviewsQuery = context.Reviews
                .Where(r => r.RestaurantId == restaurantId);

            var reviewVM = reviewsQuery.Select(r => new ReviewVM
            {
                ReviewId = r.ReviewId,
                RestaurantId = r.RestaurantId,
                AuthorId = r.AuthorId,
                AuthorFullName = r.Author.FullName,
                Stars = r.Stars,
                CreatedAt = r.CreatedAt,
                DateEdited = r.DateEdited,
                Contents = r.Contents,
                AnsweredAt = r.AnsweredAt,
                RestaurantResponse = r.RestaurantResponse
            });

            reviewVM = reviewVM.AsQueryable();
            reviewVM = orderBy switch
            {
                ReviewOrderSorting.StarsAsc => reviewVM.OrderBy(o => o.Stars),
                ReviewOrderSorting.StarsDesc => reviewVM.OrderByDescending(o => o.Stars),
                ReviewOrderSorting.DateAsc => reviewVM.OrderBy(o => o.CreatedAt),
                ReviewOrderSorting.DateDesc => reviewVM.OrderByDescending(o => o.CreatedAt),
                _ => reviewVM
            };

            return await reviewVM.PaginateAsync(page, perPage, Enum.GetNames<ReviewOrderSorting>());
        }

        /// <summary>
        /// Load review summary for a restaurant from the database
        /// </summary>
        /// <param name="restaurant">The restaurant</param>
        /// <returns>Average star count and the number of reviews</returns>
        public async Task<(double rating, int numberReviews)> GetReviewSummary(Restaurant restaurant)
        {
            var query = context.Entry(restaurant)
                .Collection(r => r.Reviews)
                .Query();

            var count = await query.CountAsync();
            return count == 0
                ? (0, 0)
                : (await query.AverageAsync(r => (double)r.Stars), count);
        }

        /// <summary>
        /// Get details about a restaurant as a not owner
        /// </summary>
        /// <param name="restaurantId">ID of the restaurant</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [ErrorCode(null, nameof(ErrorCodes.NotFound))]
        public async Task<Result<RestaurantVM>> GetRestaurantByIdAsync(int restaurantId)
        {
            var restaurant = await context.Restaurants
                .AsNoTracking()
                .OnlyActiveRestaurants()
                .Where(x => x.RestaurantId == restaurantId)
                .ProjectTo<RestaurantVM>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            if (restaurant is null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = $"Restaurant with ID {restaurantId} not found or is not verified",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            return restaurant;
        }

        /// <summary>
        /// Get visits in a restaurant
        /// </summary>
        /// <param name="userId">ID of the current user for permission checking</param>
        /// <param name="restaurantId">ID of the restaurant.</param>
        /// <param name="dateStart">Filter out visits before the date</param>
        /// <param name="dateEnd">Filter out visits after the date</param>
        /// <param name="tableId">Only visits assigned to the specified table ID</param>
        /// <param name="hasOrders">
        /// If true, only visits with orders; if false, only visits without orders; if null, all visits
        /// </param>
        /// <param name="isTakeaway">
        /// If true, only takeaway visits; if false, only dine-in visits; if null, all visits
        /// </param>
        /// <param name="visitSorting">Order visits</param>
        /// <param name="page">Page number</param>
        /// <param name="perPage">Items per page</param>
        /// <returns>Paged list of visits</returns>
        [ErrorCode(null, ErrorCodes.NotFound)]
        public async Task<Result<Pagination<VisitVM>>> GetVisitsInRestaurantAsync(
            Guid userId,
            int restaurantId,
            DateOnly? dateStart,
            DateOnly? dateEnd,
            int? tableId,
            bool? hasOrders,
            bool? isTakeaway,
            VisitSorting visitSorting,
            int page,
            int perPage)
        {
            var restaurant = await context.Restaurants
                .AsNoTracking()
                .OnlyActiveRestaurants()
                .SingleOrDefaultAsync(restaurant => restaurant.RestaurantId == restaurantId);
            if (restaurant == null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = $"Restaurant with ID {restaurantId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var authResult = await authorizationService.VerifyRestaurantHallAccess(restaurantId, userId);
            if (authResult.IsError)
            {
                return authResult.Errors;
            }

            IQueryable<Visit> query = context.Visits
                .AsSplitQuery()
                .Include(x => x.Table)
                .Include(x => x.Participants)
                .Include(x => x.Orders)
                .ThenInclude(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Where(e => e.RestaurantId == restaurantId);

            if (dateStart is not null)
            {
                query = query.Where(x => DateOnly.FromDateTime(x.Date) >= dateStart);
            }

            if (dateEnd is not null)
            {
                query = query.Where(x => DateOnly.FromDateTime(x.Date) <= dateEnd);
            }

            if (tableId is not null)
            {
                var tableExists = await context.Tables.AnyAsync(t => t.TableId == tableId && t.RestaurantId == restaurantId);
                if (!tableExists)
                {
                    return new ValidationFailure
                    {
                        PropertyName = nameof(tableId),
                        ErrorMessage = $"Table with ID {tableId} not found in restaurant {restaurantId}",
                        ErrorCode = ErrorCodes.NotFound
                    };
                }

                query = query.Where(x => x.TableId == tableId);
            }

            if (hasOrders is not null)
            {
                if (hasOrders.Value)
                {
                    query = query.Where(x => x.Orders.Count != 0);
                }
                else
                {
                    query = query.Where(x => x.Orders.Count == 0);
                }
            }

            if (isTakeaway is not null)
            {
                query = query.Where(x => x.Takeaway == isTakeaway.Value);
            }

            switch (visitSorting)
            {
                case VisitSorting.DateAsc:
                    query = query.OrderBy(e => e.Date);
                    break;
                case VisitSorting.DateDesc:
                    query = query.OrderByDescending(e => e.Date);
                    break;
                default:
                    query = query.OrderBy(e => e.Date); // Default to ascending order if VisitSorting is unexpected
                    break;
            }

            var result = await query
                .ProjectTo<VisitVM>(mapper.ConfigurationProvider)
                .PaginateAsync(page, perPage, Enum.GetNames<VisitSorting>());

            return result;
        }

        /// <summary>
        /// Get deliveries in a restaurant
        /// </summary>
        /// <param name="restaurantId">ID of the restaurant</param>
        /// <param name="returnDelivered">If true, return finished deliveries, unfinished otherwise</param>
        /// <param name="userId">Search by user ID</param>
        /// <param name="userName">Search by user name</param>
        /// <param name="orderBy">Order results by</param>
        /// <param name="currentUserId">ID of the current user for permission checks</param>
        /// <param name="page">Page number</param>
        /// <param name="perPage">Items per page</param>
        [ErrorCode(null, ErrorCodes.NotFound, "Restaurant with the given ID not found")]
        [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyRestaurantBackdoorAccess))]
        public async Task<Result<Pagination<DeliverySummaryVM>>> GetDeliveriesInRestaurantAsync(
            int restaurantId,
            bool returnDelivered,
            Guid? userId,
            string? userName,
            DeliverySorting orderBy,
            Guid currentUserId,
            int page = 0,
            int perPage = 10)
        {
            var access = await authorizationService
                .VerifyRestaurantBackdoorAccess(restaurantId, currentUserId);
            if (access.IsError)
            {
                return access.Errors;
            }

            var query = context.Deliveries
                .Where(d => d.RestaurantId == restaurantId);

            if (returnDelivered)
            {
                query = query.Where(d => d.DeliveredTime != null);
            }
            else
            {
                query = query.Where(d => d.DeliveredTime == null);
            }

            if (userId is not null)
            {
                query = query.Where(d => d.UserId == userId);
            }

            query = orderBy switch
            {
                DeliverySorting.OrderTimeAsc => query.OrderBy(d => d.OrderTime),
                DeliverySorting.OrderTimeDesc => query.OrderByDescending(d => d.OrderTime),
                DeliverySorting.DeliveredTimeAsc => query.OrderBy(d => d.DeliveredTime),
                DeliverySorting.DeliveredTimeDesc => query.OrderByDescending(d => d.DeliveredTime),
                _ => throw new ArgumentOutOfRangeException(nameof(orderBy)),
            };

            var vmQuery = query.AsDeliverySummary();

            if (userName is not null)
            {
                vmQuery = vmQuery.Where(d => d.UserFullName == null || d.UserFullName.Contains(userName));
            }

            return await vmQuery.PaginateAsync(page, perPage, Enum.GetNames<DeliverySorting>());
        }

        /// <summary>
        /// Validates and gets menu items from the given restaurant
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="restaurantId"></param>
        /// <returns>MenuItems</returns>
        public async Task<Result<List<MenuItemVM>>> GetMenuItemsOwnerAsync(Guid userId, int restaurantId)
        {
            var result = await authorizationService.VerifyOwnerRole(restaurantId, userId);
            if (result.IsError)
            {
                return result.Errors;
            }

            return await context.MenuItems
                .Where(i => i.RestaurantId == restaurantId)
                .ProjectTo<MenuItemVM>(mapper.ConfigurationProvider)
                .ToListAsync();
        }

        /// <summary>
        /// Returns a list of menus of specific restaurant (owner version)
        /// </summary>
        /// <param name="id"> Id of the restaurant.</param>
        /// <param name="userId">Id of the current user for permission checking</param>
        /// <returns></returns>
        public async Task<Result<List<MenuSummaryVM>>> GetMenusOwnerAsync(int id, Guid userId)
        {
            var restaurant = await context.Restaurants
                .AsNoTracking()
                .Include(r => r.Menus)
                .Where(i => i.RestaurantId == id)
                .FirstOrDefaultAsync();

            if (restaurant == null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(id),
                    ErrorMessage = $"Restaurant with ID {id} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var result = await authorizationService.VerifyOwnerRole(id, userId);
            if (result.IsError)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(id),
                    ErrorMessage = $"Restaurant with ID {id} does not belong to user",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            return mapper.Map<List<MenuSummaryVM>>(restaurant.Menus);
        }

        /// <summary>
        /// Get status of the ingredients in a restaurant
        /// </summary>
        /// <param name="restaurantId">ID of the restaurant</param>
        /// <param name="orderBy">Order the list by</param>
        /// <param name="page">Page number</param>
        /// <param name="perPage">Items per page</param>
        [ErrorCode(null, ErrorCodes.NotFound, "Restaurant not found")]
        [MethodErrorCodes(typeof(Utils), nameof(Utils.PaginateAsync))]
        public async Task<Result<Pagination<IngredientVM>>> GetIngredientsAsync(
            int restaurantId,
            IngredientSorting orderBy,
            int page,
            int perPage)
        {
            bool restaurantExists = await context.Restaurants
                .OnlyActiveRestaurants()
                .AnyAsync(r => r.RestaurantId == restaurantId);
            if (!restaurantExists)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = $"Restaurant with ID {restaurantId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            IQueryable<Ingredient> query = context.MenuItems
                .AsSplitQuery()
                .Where(mi => mi.RestaurantId == restaurantId)
                .SelectMany(mi => mi.Ingredients.Select(imi => imi.Ingredient))
                .Distinct();

            // Sortowanie
            query = orderBy switch
            {
                IngredientSorting.NameAsc => query.OrderBy(i => i.PublicName),
                IngredientSorting.NameDesc => query.OrderByDescending(i => i.PublicName),
                IngredientSorting.AmountAsc => query.OrderBy(i => i.Amount),
                IngredientSorting.AmountDesc => query.OrderByDescending(i => i.Amount),
                _ => throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, $"Unsupported sorting option: {orderBy}")
            };

            // Paginacja i mapowanie do IngredientVM
            var paginatedResult = await query
                .Select(i => new IngredientVM
                {
                    IngredientId = i.IngredientId,
                    PublicName = i.PublicName,
                    UnitOfMeasurement = i.UnitOfMeasurement,
                    MinimalAmount = i.MinimalAmount,
                    AmountToOrder = i.AmountToOrder,
                    Amount = i.Amount
                })
                .PaginateAsync(page, perPage, Enum.GetNames<IngredientSorting>(), maxPerPage: 20);

            return paginatedResult;
        }

        /// <summary>
        /// Get time spans on a given date that a reservation can be made in
        /// </summary>
        /// <param name="restaurantId">Restaurant ID</param>
        /// <param name="date">Date of the reservation</param>
        /// <param name="numberOfGuests">Number of people that will be going</param>
        /// <returns>Available hours list</returns>
        [ErrorCode(nameof(restaurantId), ErrorCodes.NotFound)]
        public async Task<Result<List<AvailableHoursVM>>> GetAvailableHoursAsync(int restaurantId, DateOnly date, int numberOfGuests)
        {
            var restaurant = await context.Restaurants
                .AsNoTracking()
                .Include(r => r.Tables)
                .OnlyActiveRestaurants()
                .FirstOrDefaultAsync(r => r.RestaurantId == restaurantId);

            if (restaurant == null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(restaurantId),
                    ErrorMessage = $"Restaurant with ID {restaurantId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            // Pobieramy stoliki odpowiednie do liczby gości
            var availableTables = await context.Tables
                .Where(t => t.RestaurantId == restaurantId && t.Capacity >= numberOfGuests)
                .ToListAsync();

            // Pobieramy rezerwacje, które kolidują z danym dniem
            var reservations = await context.Visits
                .Where(v => v.RestaurantId == restaurantId && v.Date.Date == date.ToDateTime(new TimeOnly()))
                .ToListAsync();

            //Pobranie godzin otwarcia i zamknięcia restauracji w podanym dniu
            var openingTime = restaurant.OpeningHours[date.DayOfWeek].From;
            var closingTime = restaurant.OpeningHours[date.DayOfWeek].Until;

            if (openingTime == null || closingTime == null)
            {
                return new List<AvailableHoursVM>();
            }

            var availableHours = new List<AvailableHoursVM>();

            // Sprawdzamy dostępność godzin dla każdego stolika
            var wrappedDays = 0;
            for (var time = openingTime.Value; time < closingTime.Value && wrappedDays < 1; time = time.AddMinutes(30, out wrappedDays))
            {
                var timeSlotAvailable = availableTables.Any(table =>
                    // Check if there are no reservations for this table that overlap with the current time slot
                    !reservations.Any(r =>
                        r.TableId == table.TableId && // Same table
                        TimeOnly.FromDateTime(r.Date) <= time && // Reservation starts before or at the current time (compare times only)
                        TimeOnly.FromDateTime(r.EndTime) > time // Reservation ends after the current time slot starts (compare times only)
                    )
                );

                if (timeSlotAvailable)
                {
                    availableHours.Add(new AvailableHoursVM
                    {
                        From = time,
                        Until = time.Add(TimeSpan.FromMinutes(30))
                    });
                }
            }

            // Łączenie sąsiadujących przedziałów czasowych
            var mergedAvailableHours = new List<AvailableHoursVM>();
            AvailableHoursVM? currentSlot = null;

            foreach (var slot in availableHours)
            {
                if (currentSlot == null)
                {
                    currentSlot = slot;
                }
                else if (currentSlot.Until == slot.From)
                {
                    // Przedziały sąsiadują, więc łączymy je
                    currentSlot.Until = slot.Until;
                }
                else
                {
                    // Zapisujemy połączony przedział i zaczynamy nowy
                    mergedAvailableHours.Add(currentSlot);
                    currentSlot = slot;
                }
            }

            // Dodajemy ostatni połączony przedział (jeśli istnieje)
            if (currentSlot != null)
            {
                mergedAvailableHours.Add(currentSlot);
            }

            return mergedAvailableHours;
        }

        /// <summary>
        /// Get list of restaurant's current employees
        /// </summary>
        /// <param name="restaurantId">ID of the restaurants</param>
        /// <param name="userId">ID of the current user (to check permissions)</param>
        [ErrorCode(null, ErrorCodes.NotFound)]
        public async Task<Result<List<LimitedEmployeeVM>>> GetEmployeesLimitedAsync(int restaurantId, Guid userId)
        {
            var restaurant = await context.Restaurants
                .AsNoTracking()
                .Include(r => r.Group)
                .Where(r => r.RestaurantId == restaurantId)
                .FirstOrDefaultAsync();

            if (restaurant == null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(restaurantId),
                    ErrorMessage = $"Restaurant with ID {restaurantId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var authResult = await authorizationService.VerifyRestaurantHallAccess(restaurantId, userId);
            if (authResult.IsError)
            {
                return authResult.Errors;
            }

            var employees = await context.Employments
                .AsNoTracking()
                .Where(e => e.RestaurantId == restaurantId && e.DateUntil == null)
                .Select(e => new LimitedEmployeeVM
                {
                    EmployeeId = e.EmployeeId,
                    FirstName = e.Employee.FirstName,
                    LastName = e.Employee.LastName,
                    IsHallEmployee = e.IsHallEmployee,
                    IsBackdoorEmployee = e.IsBackdoorEmployee
                })
                .ToListAsync();

            return employees;

        }
    }
}
