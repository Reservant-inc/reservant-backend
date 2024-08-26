using ErrorCodeDocs.Attributes;
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
    public class ReviewService(
        UserManager<User> userManager,
        ApiDbContext context,
        ValidationService validationService)
    {
        [ErrorCode(null, ErrorCodes.NotFound)]
        public async Task<Result> DeleteReviewAsync(int id, User user)
        {
            var review = await context.Reviews.FirstOrDefaultAsync(r => r.AuthorId == user.Id && r.Id == id);
            if (review == null)
            {
                return new ValidationFailure
                {
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var restaurant = await context.Restaurants.Include(r => r.Reviews).FirstOrDefaultAsync(r => r.Id == review.RestaurantId);

            if (restaurant is not null)
            {
                restaurant.Reviews.Remove(review);
            }
            context.Reviews.Remove(review);
            await context.SaveChangesAsync();
            return Result.Success;
        }

        [ErrorCode(null, ErrorCodes.NotFound)]
        [ValidatorErrorCodes<CreateReviewRequest>]
        public async Task<Result<ReviewVM>> UpdateReviewAsync(int id, User user, CreateReviewRequest request)
        {
            var res = await validationService.ValidateAsync(request, user.Id);
            if (!res.IsValid)
            {
                return res.Errors;
            }
            var review = await context.Reviews.FirstOrDefaultAsync(r => r.AuthorId == user.Id && r.Id == id);
            if (review == null)
            {
                return new ValidationFailure
                {
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            review.Stars = request.Stars;
            review.Contents = request.Contents;

            await context.SaveChangesAsync();
            return new ReviewVM
            {
                ReviewId = review.Id,
                RestaurantId = review.Id,
                Stars = review.Stars,
                AuthorId = user.Id,
                AuthorFullName = user.FullName,
                CreatedAt = DateTime.UtcNow,
                Contents = review.Contents,
                AnsweredAt = review.AnsweredAt,
                RestaurantResponse = review.RestaurantResponse
            };
        }
    }
}
