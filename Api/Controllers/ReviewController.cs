﻿using Reservant.ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos.Review;
using Reservant.Api.Identity;
using Reservant.Api.Services;

namespace Reservant.Api.Controllers
{
    /// <summary>
    /// Managing reviews
    /// </summary>
    [ApiController, Route("/reviews")]
    public class ReviewController(ReviewService reviewService) : StrictController
    {
        /// <summary>
        /// Deletes a review with a given id, great for censorship
        /// </summary>
        /// <param name="reviewId">id of the review</param>
        /// <returns>confirmation of the action's status</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Customer)]
        [MethodErrorCodes<ReviewService>(nameof(ReviewService.DeleteReviewAsync))]
        [ProducesResponseType(204), ProducesResponseType(400)]
        public async Task<ActionResult> DeleteReview(int reviewId) {

            var userid = User.GetUserId();

            var result = await reviewService.DeleteReviewAsync(reviewId, userid!);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Updates a review with given id
        /// </summary>
        /// <param name="reviewId">id of the review to change</param>
        /// <param name="request">new contents of the review</param>
        /// <returns>a visual model of the updated review</returns>
        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.Customer)]
        [MethodErrorCodes<ReviewService>(nameof(ReviewService.UpdateReviewAsync))]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult<ReviewVM>> UpdateReview(int reviewId, CreateReviewRequest request)
        {
            var userid = User.GetUserId();


            var result = await reviewService.UpdateReviewAsync(reviewId, userid!, request);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Gets a review with a given id
        /// </summary>
        /// <param name="id">id of the review</param>
        /// <returns>a visual model of the review</returns>
        [HttpGet("{id:int}")]
        [MethodErrorCodes<ReviewService>(nameof(ReviewService.GetReviewAsync))]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult<ReviewVM>> GetReview(int id)
        {
            var result = await reviewService.GetReviewAsync(id);
            return OkOrErrors(result);
        }

    }
}
