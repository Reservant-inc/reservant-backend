using Reservant.ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos.Reviews;
using Reservant.Api.Identity;
using Reservant.Api.Services;
using Reservant.Api.Models;
using Microsoft.AspNetCore.Identity;


namespace Reservant.Api.Controllers
{
    /// <summary>
    /// Managing reviews
    /// </summary>
    [ApiController, Route("/reviews")]
    public class ReviewController(
        ReviewService reviewService//,
        //UserManager<User> userManager
        ) : StrictController
    {
        /// <summary>
        /// Deletes a review with a given id, great for censorship
        /// </summary>
        /// <param name="reviewId">id of the review</param>
        /// <returns>confirmation of the action's status</returns>
        [HttpDelete("{reviewId:int}")]
        [Authorize(Roles = Roles.Customer)]
        [MethodErrorCodes<ReviewService>(nameof(ReviewService.DeleteReviewAsync))]
        [ProducesResponseType(204), ProducesResponseType(400)]
        public async Task<ActionResult> DeleteReview(int reviewId) {

            var userid = User.GetUserId();

            var result = await reviewService.DeleteReviewAsync(reviewId, userid!.Value);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Updates a review with given id
        /// </summary>
        /// <param name="reviewId">id of the review to change</param>
        /// <param name="request">new contents of the review</param>
        /// <returns>a visual model of the updated review</returns>
        [HttpPut("{reviewId:int}")]
        [Authorize(Roles = Roles.Customer)]
        [MethodErrorCodes<ReviewService>(nameof(ReviewService.UpdateReviewAsync))]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult<ReviewVM>> UpdateReview(int reviewId, CreateReviewRequest request)
        {
            var userid = User.GetUserId();


            var result = await reviewService.UpdateReviewAsync(reviewId, userid!.Value, request);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Gets a review with a given id
        /// </summary>
        /// <param name="reviewId">id of the review</param>
        /// <returns>a visual model of the review</returns>
        [HttpGet("{reviewId:int}")]
        [MethodErrorCodes<ReviewService>(nameof(ReviewService.GetReviewAsync))]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult<ReviewVM>> GetReview(int reviewId)
        {
            var result = await reviewService.GetReviewAsync(reviewId);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Adds or updates a restaurant response to a review with a given id
        /// </summary>
        /// <param name="reviewId">id of the review</param>
        /// <param name="restaurnatResponse">the restaurant response</param>
        /// <returns>a visual model of the updated review</returns>
        [HttpPut("{reviewId:int}/restaurant-response")]
        [Authorize(Roles = Roles.RestaurantOwner)]
        [MethodErrorCodes<ReviewService>(nameof(ReviewService.UpdateRestaurantResponseAsync))]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult<ReviewVM>> UpdateRestaurantResponse(int reviewId, RestaurantResponseDto restaurnatResponse)
        {
            var userId = User.GetUserId();
            // var user = await userManager.GetUserAsync(User);
            // if (user is null)
            // {
            //     return Unauthorized();
            // }

            var result = await reviewService.UpdateRestaurantResponseAsync(reviewId, userId!.Value, restaurnatResponse);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Deletes a restaurant response from a review with a given id
        /// </summary>
        /// <param name="reviewId">id of the review</param>
        /// <returns>confirmation of the action's status</returns>
        [HttpDelete("{reviewId:int}/restaurant-response")]
        [Authorize(Roles = Roles.RestaurantOwner)]
        [MethodErrorCodes<ReviewService>(nameof(ReviewService.DeleteRestaurantResponseAsync))]
        [ProducesResponseType(204), ProducesResponseType(400)]
        public async Task<ActionResult> DeleteRestaurantResponse(int reviewId)
        {
            var userId = User.GetUserId();
            //var user = await userManager.GetUserAsync(User);
            // if (userId is null)
            // {
            //     return Unauthorized();
            // }

            var result = await reviewService.DeleteRestaurantResponseAsync(reviewId, userId!.Value);
            return OkOrErrors(result);
        }
    
    }
}
