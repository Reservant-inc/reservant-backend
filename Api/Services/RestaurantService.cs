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
using Reservant.Api.Models.Dtos.Review;
using Reservant.Api.Models.Enums;
using Reservant.Api.Validators;
using Reservant.Api.Models.Dtos.Event;
using Reservant.Api.Models.Dtos.Visit;
using Reservant.Api.Models.Dtos.User;



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
        ValidationService validationService,
        GeometryFactory geometryFactory)
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
        /// <param name="page">Page number</param>
        /// <param name="perPage">Items per page</param>
        /// <param name="lat1">Search within a rectengular area: first point's latitude</param>
        /// <param name="lon1">Search within a rectengular area: first point's longitude</param>
        /// <param name="lat2">Search within a rectengular area: second point's latitude</param>
        /// <param name="lon2">Search within a rectengular area: second point's longitude</param>
        /// <returns></returns>
        public async Task<Result<Pagination<NearRestaurantVM>>> FindRestaurantsAsync(
            double? origLat, double? origLon,
            string? name, HashSet<string> tags,
            double? lat1, double? lon1, double? lat2, double? lon2,
            int page, int perPage)
        {
            IQueryable<Restaurant> query = context.Restaurants
                .Where(r => r.VerifierId != null);

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
                    RestaurantId = r.Id,
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
                    Logo = uploadService.GetPathForFileName(r.LogoFileName),
                    Description = r.Description,
                    ReservationDeposit = r.ReservationDeposit,
                    Tags = r.Tags.Select(t => t.Name).ToList(),
                    DistanceFrom = origin.IsEmpty ? null : origin.Distance(r.Location),
                    Rating = r.Reviews.Select(rv => (double?)rv.Stars).Average() ?? 0,
                    NumberReviews = r.Reviews.Count
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
                Location = geometryFactory.CreatePoint(new Coordinate(request.Location.Longitude, request.Location.Latitude)),
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

            return new MyRestaurantVM
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
                    Longitude = restaurant.Location.Y,
                    Latitude = restaurant.Location.X
                },
                ReservationDeposit = restaurant.ReservationDeposit
            };
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
                .Where(r => r.Group.OwnerId == userId)
                .Include(r => r.Reviews)
                .Where(r => name == null || r.Name.Contains(name.Trim()))
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
                        Longitude = r.Location.Y,
                        Latitude = r.Location.X
                    },
                    GroupId = r.GroupId,
                    ProvideDelivery = r.ProvideDelivery,
                    Logo = uploadService.GetPathForFileName(r.LogoFileName),
                    Description = r.Description,
                    ReservationDeposit = r.ReservationDeposit,
                    Tags = r.Tags.Select(t => t.Name).ToList(),
                    IsVerified = r.VerifierId != null,
                    Rating = r.Reviews.Average(review => review.Stars),
                    NumberReviews = r.Reviews.Count
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
        public async Task<MyRestaurantVM?> GetMyRestaurantByIdAsync(User user, int id)
        {
            var userId = user.Id;
            var result = await context.Restaurants
                .Where(r => r.Group.OwnerId == userId)
                .Where(r => r.Id == id)
                .Select(r => new MyRestaurantVM
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
                        Longitude = r.Location.Y,
                        Latitude = r.Location.X
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
                DateFrom = DateOnly.FromDateTime(DateTime.UtcNow)
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
                .Include(r => r.Reviews)
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
                    Longitude = restaurant.Location.Y,
                    Latitude = restaurant.Location.X
                },
                GroupId = restaurant.GroupId,
                Description = restaurant.Description,
                ReservationDeposit = restaurant.ReservationDeposit,
                Logo = uploadService.GetPathForFileName(restaurant.LogoFileName),
                Tags = restaurant.Tags.Select(t => t.Name).ToList(),
                ProvideDelivery = restaurant.ProvideDelivery,
                IsVerified = restaurant.VerifierId != null,
                Rating = restaurant.Rating,
                NumberReviews = restaurant.Reviews.Count
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
        public async Task<Result<MyRestaurantVM>> UpdateRestaurantAsync(int id, UpdateRestaurantRequest request, User user)
        {
            var restaurant = await context.Restaurants
                .AsSplitQuery()
                .Include(restaurant => restaurant.Group)
                .Include(restaurant => restaurant.Tables)
                .Include(restaurant => restaurant.Photos)
                .Include(restaurant => restaurant.Tags)
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

            restaurant.Name = request.Name.Trim();
            restaurant.Nip = request.Nip.Trim();
            restaurant.RestaurantType = request.RestaurantType;
            restaurant.Address = request.Address.Trim();
            restaurant.PostalIndex = request.PostalIndex.Trim();
            restaurant.City = request.City.Trim();
            restaurant.ProvideDelivery = request.ProvideDelivery;
            restaurant.Description = request.Description?.Trim();
            restaurant.ReservationDeposit = request.ReservationDeposit;

            restaurant.RentalContractFileName = request.RentalContract;
            restaurant.AlcoholLicenseFileName = request.AlcoholLicense;
            restaurant.BusinessPermissionFileName = request.BusinessPermission;
            restaurant.IdCardFileName = request.IdCard;
            restaurant.LogoFileName = request.Logo;

            restaurant.Tags = await context.RestaurantTags
                .Where(t => request.Tags.Contains(t.Name))
                .ToListAsync();

            var photos = request.Photos
                .Select((photo, index) => new RestaurantPhoto
                {
                    PhotoFileName = photo,
                    Order = index + 1
                }).ToList();

            restaurant.Photos=photos;

            result = await validationService.ValidateAsync(restaurant, user.Id);
            if (!result.IsValid)
            {
                return result;
            }

            await context.SaveChangesAsync();

            return new MyRestaurantVM
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
                    Longitude = restaurant.Location.Y,
                    Latitude = restaurant.Location.X
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
                    DateUntil = menu.DateUntil,
                    Photo = uploadService.GetPathForFileName(menu.PhotoFileName)
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
            context.RemoveRange(restaurant.MenuItems);
            context.RemoveRange(restaurant.Menus);

            context.Remove(restaurant);
            // We check if the restaurant was the last one (the collection was loaded before we deleted it)
            if (restaurant.Group.Restaurants.Count == 1)
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

            var restaurant = await context.Restaurants
                .Include(restaurant => restaurant.Group)
                .FirstOrDefaultAsync(x => x.Id == restaurantId);
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

            return await filteredOrders.PaginateAsync(page, perPage, Enum.GetNames<OrderSorting>());
        }

        /// <summary>
        /// Get future events in a restaurant with pagination.
        /// </summary>
        /// <param name="restaurantId">ID of the restaurant.</param>
        /// <param name="page">Page number to return.</param>
        /// <param name="perPage">Items per page.</param>
        /// <returns>Paginated list of future events.</returns>
        public async Task<Result<Pagination<EventSummaryVM>>> GetFutureEventsByRestaurantAsync(int restaurantId, int page, int perPage)
        {
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

            var query = context.Events
                .Where(e => e.RestaurantId == restaurantId && e.Time > DateTime.UtcNow)
                .OrderBy(e => e.Time)
                .Select(e => new EventSummaryVM
                {
                    EventId = e.Id,
                    Description = e.Description,
                    Time = e.Time,
                    MustJoinUntil = e.MustJoinUntil,
                    CreatorId = e.CreatorId,
                    CreatorFullName = e.Creator.FullName,
                    RestaurantId = e.RestaurantId,
                    RestaurantName = e.Restaurant.Name,
                    NumberInterested = e.Interested.Count
                });

            return await query.PaginateAsync(page, perPage, []);
        }

        /// <summary>
        /// Add review to restaurant of given id from logged in user containing data from request
        /// </summary>
        /// <param name="user">User putting in a review</param>
        /// <param name="restaurantId">ID of restaurant reciving review</param>
        /// <param name="createReviewRequest">template for data provided in a reveiw</param>
        /// <returns>View of a created review</returns>
        public async Task<Result<ReviewVM>> CreateReviewAsync(User user, int restaurantId, CreateReviewRequest createReviewRequest)
        {
            var restaurant = await context.Restaurants
                .Where(r => r.Id == restaurantId)
                .FirstOrDefaultAsync();

            if (restaurant == null)
            {
                return new ValidationFailure { PropertyName = null, ErrorCode = ErrorCodes.NotFound };
            }

            var createReviewRequestValidation = await validationService.ValidateAsync(createReviewRequest,user.Id);
            if(!createReviewRequestValidation.IsValid)
            {
                return createReviewRequestValidation;
            }

            var existingReview = await context.Reviews
                .Where(r => r.RestaurantId == restaurantId)
                .Where(r => r.Author==user)
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


            var reviewValidation = await validationService.ValidateAsync(newReview,user.Id);
            if(!reviewValidation.IsValid)
            {
                return reviewValidation;
            }

            await context.Reviews.AddAsync(newReview);
            await context.SaveChangesAsync();

            var reviewVM = new ReviewVM
            {
                ReviewId = newReview.Id,
                RestaurantId=newReview.RestaurantId,
                AuthorId=newReview.AuthorId,
                AuthorFullName=newReview.Author.FullName,
                Stars=newReview.Stars,
                CreatedAt=newReview.CreatedAt,
                Contents=newReview.Contents,
                AnsweredAt=newReview.AnsweredAt,
                RestaurantResponse=newReview.RestaurantResponse
            };

            return reviewVM;
        }

        /// <summary>
        /// Get reviews for a restaurant
        /// </summary>
        public async Task<Result<Pagination<ReviewVM>>> GetReviewsAsync(int restaurantId, ReviewOrderSorting orderBy = ReviewOrderSorting.DateDesc, int page = 0, int perPage = 10)
        {
            var restaurant = await context.Restaurants.FindAsync(restaurantId);

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
                ReviewId = r.Id,
                RestaurantId = r.RestaurantId,
                AuthorId = r.AuthorId,
                AuthorFullName = r.Author.FullName,
                Stars = r.Stars,
                CreatedAt = r.CreatedAt,
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
        public async Task<Result<RestaurantVM>> GetRestaurantByIdAsync(int restaurantId)
        {
            var restaurant = await context.Restaurants
                .Include(restaurant => restaurant.Tables)
                .Include(restaurant => restaurant.Photos)
                .Include(restaurant => restaurant.Tags)
                .FirstOrDefaultAsync(x => x.Id == restaurantId && x.VerifierId != null);
            if (restaurant is null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = $"Restaurant with ID {restaurantId} not found or is not verified",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var (rating, numberReviews) = await GetReviewSummary(restaurant);

            return new RestaurantVM
            {
                RestaurantId = restaurant.Id,
                Name = restaurant.Name,
                RestaurantType = restaurant.RestaurantType,
                Address = restaurant.Address,
                PostalIndex = restaurant.PostalIndex,
                City = restaurant.City,
                Location = new Geolocation
                {
                    Latitude = restaurant.Location.Y,
                    Longitude = restaurant.Location.X
                },
                Tables = restaurant.Tables.Select(x => new TableVM
                {
                    Capacity = x.Capacity,
                    TableId = x.Id
                }).ToList(),
                ProvideDelivery = restaurant.ProvideDelivery,
                Logo = uploadService.GetPathForFileName(restaurant.LogoFileName),
                Photos = restaurant.Photos
                    .Select(x => uploadService.GetPathForFileName(x.PhotoFileName))
                    .ToList(),
                Description = restaurant.Description,
                ReservationDeposit = restaurant.ReservationDeposit,
                Tags = restaurant.Tags.Select(x => x.Name).ToList(),
                Rating = rating,
                NumberReviews = numberReviews,
            };
        }

        /// <summary>
        /// Get visits in a restaurant
        /// </summary>
        /// <param name="restaurantId">ID of the restaurant.</param>
        /// <param name="dateStart">Filter out visits before the date</param>
        /// <param name="dateEnd">Filter out visits ater the date</param>
        /// <param name="visitSorting">Order visits</param>
        /// <param name="page">Page number</param>
        /// <param name="perPage">Items per page</param>
        /// <returns>Paged list of visits</returns>
        public async Task<Result<Pagination<VisitVM>>> GetVisitsInRestaurantAsync(
            int restaurantId,
            DateOnly? dateStart,
            DateOnly? dateEnd,
            VisitSorting visitSorting,
            int page,
            int perPage)
        {
            var restaurant = await context.Restaurants.FindAsync(restaurantId);
            if (restaurant == null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = $"Restaurant with ID {restaurantId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            IQueryable<Visit> query = context.Visits
                .AsSplitQuery()
                .Include(x => x.Table)
                .Include(x => x.Participants)
                .Include(x => x.Orders)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.MenuItem)
                .Where(e => e.TableRestaurantId == restaurantId);

            if (dateStart is not null)
            {
                query = query.Where(x => DateOnly.FromDateTime(x.Date) >= dateStart);
            }

            if (dateEnd is not null)
            {
                query = query.Where(x => DateOnly.FromDateTime(x.Date) <= dateEnd);
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

            var result = await query.Select(e => new VisitVM
                {
                    VisitId = e.Id,
                    Date = e.Date,
                    NumberOfGuests = e.NumberOfGuests,
                    PaymentTime = e.PaymentTime,
                    Deposit = e.Deposit,
                    ReservationDate = e.ReservationDate,
                    Tip = e.Tip,
                    Takeaway = e.Takeaway,
                    ClientId = e.ClientId,
                    RestaurantId = e.Table.RestaurantId,
                    TableId = e.Table.Id,
                    Participants = e.Participants.Select(p => new UserSummaryVM
                    {
                        UserId = p.Id,
                        FirstName = p.FirstName,
                        LastName = p.LastName
                    }).ToList(),
                    Orders = e.Orders.Select(o => new OrderSummaryVM
                    {
                        OrderId = o.Id,
                        VisitId = o.VisitId,
                        Date = o.Visit.Date,
                        Note = o.Note,
                        Cost = o.Cost, // This now safely computes Cost
                        Status = o.Status
                    }).ToList()
                })
                .PaginateAsync(page, perPage, Enum.GetNames<VisitSorting>());

            return result;
        }
    }
}
