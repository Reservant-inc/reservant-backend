using ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos.Review;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Validators;

namespace Reservant.Api.Controllers
{
    /// <summary>
    /// Managing reviews
    /// </summary>
    [ApiController, Route("/reviews")]
    [Authorize]
    public class ReviewController(ReviewService reviewService, UserManager<User> userManager) : StrictController
    {
        /// <summary>
        /// Deletes a review with a given id, great for censorship
        /// </summary>
        /// <param name="id">id of the review</param>
        /// <returns>confirmation of the action's status</returns>
        [HttpDelete("{id:int}")]
        [MethodErrorCodes<ReviewService>(nameof(ReviewService.DeleteReviewAsync))]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult> DeleteReview(int id) {

            var userid = User.GetUserId();

            var result = await reviewService.DeleteReviewAsync(id, userid);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Updates a review with given id
        /// </summary>
        /// <param name="id">id of the review to change</param>
        /// <param name="request">new contents of the review</param>
        /// <returns>a visual model of the updated review</returns>
        [HttpPut("{id:int}")]
        [Authorize]
        [MethodErrorCodes<ReviewService>(nameof(ReviewService.UpdateReviewAsync))]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult<ReviewVM>> UpdateReview(int id, CreateReviewRequest request)
        {
            var userid = User.GetUserId();


            var result = await reviewService.UpdateReviewAsync(id, userid, request);
            return OkOrErrors(result);
        }
    }
}
