using Reservant.ErrorCodeDocs.Attributes;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Reviews;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Reservant.Api.Dtos;

namespace Reservant.Api.Services
{
    /// <summary>
    /// Service for managing reviews
    /// </summary>
    public class ReviewService(
        ApiDbContext context,
        ValidationService validationService,
        AuthorizationService authorizationService,
        IMapper mapper
        )
    {
        /// <summary>
        /// Delete review by ID
        /// </summary>
        /// <param name="reivewId">ID of the review</param>
        /// <param name="userid">ID of the current user for permission checking</param>
        /// <returns></returns>
        [ErrorCode(null, ErrorCodes.NotFound)]
        [ErrorCode(null, ErrorCodes.AccessDenied)]
        public async Task<Result> DeleteReviewAsync(int reivewId, Guid userid)
        {
            var review = await context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reivewId);
            if (review == null)
            {
                return new ValidationFailure
                {
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            if (review.AuthorId != userid)
            {
                return new ValidationFailure
                {
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }

            review.IsDeleted = true;
            await context.SaveChangesAsync();
            return Result.Success;
        }

        /// <summary>
        /// Update review by ID
        /// </summary>
        /// <param name="reviewId">ID of the review</param>
        /// <param name="userid">ID of the current user for permission checking</param>
        /// <param name="request">New review information</param>
        /// <returns></returns>
        [ErrorCode(null, ErrorCodes.NotFound)]
        [ErrorCode(null, ErrorCodes.AccessDenied)]
        [ValidatorErrorCodes<CreateReviewRequest>]
        [ValidatorErrorCodes<Review>]
        public async Task<Result<ReviewVM>> UpdateReviewAsync(int reviewId, Guid userid, CreateReviewRequest request)
        {
            var res = await validationService.ValidateAsync(request, userid);
            if (!res.IsValid)
            {
                return res.Errors;
            }
            var review = await context.Reviews.Include(r => r.Author).FirstOrDefaultAsync(r => r.ReviewId == reviewId);
            if (review == null)
            {
                return new ValidationFailure
                {
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            if (review.AuthorId != userid)
            {
                return new ValidationFailure
                {
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }

            review.Stars = request.Stars;
            review.Contents = request.Contents;
            review.DateEdited = DateTime.UtcNow;

            res = await validationService.ValidateAsync(review, userid);
            if (!res.IsValid)
            {
                return res.Errors;
            }

            await context.SaveChangesAsync();
            return new ReviewVM
            {
                ReviewId = review.ReviewId,
                RestaurantId = review.RestaurantId,
                Stars = review.Stars,
                AuthorId = review.AuthorId,
                AuthorFullName = review.Author.FullName,
                CreatedAt = review.CreatedAt,
                DateEdited = review.DateEdited,
                Contents = review.Contents,
                AnsweredAt = review.AnsweredAt,
                RestaurantResponse = review.RestaurantResponse
            };
        }

        /// <summary>
        /// Get review by ID
        /// </summary>
        /// <param name="reviewId">ID of the review</param>
        /// <returns></returns>
        [ErrorCode(null, ErrorCodes.NotFound)]
        public async Task<Result<ReviewVM>> GetReviewAsync(int reviewId)
        {
            var review = await context.Reviews
                .Select(review => new ReviewVM
                {
                    ReviewId = review.ReviewId,
                    RestaurantId = review.RestaurantId,
                    Stars = review.Stars,
                    AuthorId = review.AuthorId,
                    AuthorFullName = review.Author.FullName,
                    CreatedAt = review.CreatedAt,
                    DateEdited = review.DateEdited,
                    Contents = review.Contents,
                    AnsweredAt = review.AnsweredAt,
                    RestaurantResponse = review.RestaurantResponse
                })
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);
            if (review == null)
            {
                return new ValidationFailure
                {
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            return review;
        }

        /// <summary>
        /// Update restaurant response by review ID
        /// </summary>
        /// <param name="reviewId">ID of the review</param>
        /// <param name="userId">Id of the current user for permission checking</param>
        /// <param name="restaurantResponse">New restaurant response</param>
        /// <returns></returns>
        [ErrorCode(null, ErrorCodes.NotFound)]
        [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyOwnerRole))]
        [ValidatorErrorCodes<Review>]
        public async Task<Result<ReviewVM>> UpdateRestaurantResponseAsync(int reviewId,Guid userId, RestaurantResponseDto restaurantResponse)
        {
            var review = await context.Reviews
                .Include(r => r.Author)
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);
            if (review == null)
            {
                return new ValidationFailure
                {
                    ErrorCode = ErrorCodes.NotFound,
                };
            }

            var authResult = await authorizationService.VerifyOwnerRole(review.RestaurantId, userId);
            if (authResult.IsError)
            {
                return authResult.Errors;
            }

            review.RestaurantResponse = restaurantResponse.RestaurantResponseText;
            review.AnsweredAt = DateTime.UtcNow;

            var res = await validationService.ValidateAsync(review, userId);
            if (!res.IsValid)
            {
                return res.Errors;
            }

            await context.SaveChangesAsync();
            return mapper.Map<ReviewVM>(review);
        }

        /// <summary>
        /// Delete restaurant response by review ID
        /// </summary>
        /// <param name="reviewId">ID of the review</param>
        /// <param name="userId">Id of the Current user for permission checking</param>
        /// <returns></returns>
        [ErrorCode(null, ErrorCodes.NotFound)]
        [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyOwnerRole))]
        public async Task<Result> DeleteRestaurantResponseAsync(int reviewId, Guid userId)
        {
            var review = await context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
            if (review == null)
            {
                return new ValidationFailure
                {
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var authResult = await authorizationService.VerifyOwnerRole(review.RestaurantId, userId);
            if (authResult.IsError)
            {
                return authResult.Errors;
            }

            review.RestaurantResponse = null;
            review.AnsweredAt = null;
            await context.SaveChangesAsync();
            return Result.Success;
        }

        /// <summary>
        /// Get reviews authored by the given user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="page">Page number</param>
        /// <param name="perPage">Per page</param>
        [ErrorCode(null, ErrorCodes.NotFound)]
        public async Task<Result<Pagination<ReviewVM>>> GetReviewsOfUser(Guid userId, int page, int perPage)
        {
            var author = await context.Users.FindAsync(userId);
            if (author is null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(userId),
                    ErrorCode = ErrorCodes.NotFound,
                    ErrorMessage = $"User with ID {userId} not found",
                };
            }

            return await context.Reviews
                .Where(review => review.Author == author)
                .ProjectTo<ReviewVM>(mapper.ConfigurationProvider)
                .PaginateAsync(page, perPage, []);
        }
    }
}
