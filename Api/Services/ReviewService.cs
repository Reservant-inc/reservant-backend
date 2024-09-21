using Reservant.ErrorCodeDocs.Attributes;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Review;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

namespace Reservant.Api.Services
{
    /// <summary>
    /// Service for managing reviews
    /// </summary>
    public class ReviewService(
        ApiDbContext context,
        ValidationService validationService)
    {
        /// <summary>
        /// Delete review by ID
        /// </summary>
        /// <param name="reivewId">ID of the review</param>
        /// <param name="userid">ID of the current user for permission checking</param>
        /// <returns></returns>
        [ErrorCode(null, ErrorCodes.NotFound)]
        [ErrorCode(null, ErrorCodes.AccessDenied)]
        public async Task<Result> DeleteReviewAsync(int reivewId, string userid)
        {
            var review = await context.Reviews.FirstOrDefaultAsync(r => r.Id == reivewId);
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
        public async Task<Result<ReviewVM>> UpdateReviewAsync(int reviewId, string userid, CreateReviewRequest request)
        {
            var res = await validationService.ValidateAsync(request, userid);
            if (!res.IsValid)
            {
                return res.Errors;
            }
            var review = await context.Reviews.Include(r => r.Author).FirstOrDefaultAsync(r => r.Id == reviewId);
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

            res = await validationService.ValidateAsync(review, userid);
            if (!res.IsValid)
            {
                return res.Errors;
            }

            await context.SaveChangesAsync();
            return new ReviewVM
            {
                ReviewId = review.Id,
                RestaurantId = review.Id,
                Stars = review.Stars,
                AuthorId = review.AuthorId,
                AuthorFullName = review.Author.FullName,
                CreatedAt = DateTime.UtcNow,
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
                    ReviewId = review.Id,
                    RestaurantId = review.Id,
                    Stars = review.Stars,
                    AuthorId = review.AuthorId,
                    AuthorFullName = review.Author.FullName,
                    CreatedAt = review.CreatedAt,
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
    }
}
