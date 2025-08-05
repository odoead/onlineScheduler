using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewService.DTO;
using ReviewService.Interface;

namespace RewievService.Controllers
{    

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService reviewService;
        private readonly ILogger<ReviewsController> logger;

        public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
        {
            this.reviewService = reviewService;
            this.logger = logger;
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "ReviewReadPolicy")]
        public async Task<ActionResult<ReviewDTO>> GetReview(int id)
        {
            var review = await reviewService.GetReviewByIdAsync(id);
            if (review == null)
                return NotFound();

            return Ok(review);
        }

        [HttpGet("client")]
        [Authorize(Policy = "ReviewReadPolicy")]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetClientReviews()
        {
            var clientId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(clientId))
                return Unauthorized();

            var reviews = await reviewService.GetReviewsByClientIdAsync(clientId);
            return Ok(reviews);
        }

        [HttpGet("target")]
        [Authorize(Policy = "ReviewReadPolicy")]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetTargetReviews(
            [FromQuery] string targetId,
            [FromQuery] string targetType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrEmpty(targetId) || string.IsNullOrEmpty(targetType))
                return BadRequest("Target ID and Target Type are required");

            var filter = new ReviewFilterDTO
            {
                TargetId = targetId,
                TargetType = targetType,
                Page = page,
                PageSize = pageSize
            };

            var (reviews, totalCount) = await reviewService.GetFilteredReviewsAsync(filter);

            Response.Headers.Add("X-Total-Count", totalCount.ToString());

            return Ok(reviews);
        }

        [HttpGet("summary")]
        [AllowAnonymous]
        public async Task<ActionResult<ReviewSummaryDTO>> GetReviewSummary(
            [FromQuery] string targetId,
            [FromQuery] string targetType)
        {
            if (string.IsNullOrEmpty(targetId) || string.IsNullOrEmpty(targetType))
                return BadRequest("Target ID and Target Type are required");

            var summary = await reviewService.GetReviewSummaryAsync(targetId, targetType);
            return Ok(summary);
        }

        [HttpPost]
        [Authorize(Policy = "ReviewWritePolicy")]
        public async Task<ActionResult<ReviewDTO>> SubmitReview(SubmitReviewDTO reviewDto)
        {
            try
            {
                var clientId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(clientId))
                    return Unauthorized();

                // Validate target type
                if (reviewDto.TargetType != "WORKER" && reviewDto.TargetType != "PRODUCT")
                    return BadRequest("Invalid target type. Must be either 'WORKER' or 'PRODUCT'");

                // Check if client can review this target
                var canReview = await reviewService.CanClientReviewTargetAsync(
                    clientId, reviewDto.TargetId, reviewDto.TargetType);

                if (!canReview)
                    return BadRequest("You have already reviewed this target");

                var review = await reviewService.SubmitReviewAsync(clientId, reviewDto);
                return CreatedAtAction(nameof(GetReview), new { id = review.Id }, review);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error submitting review");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/respond")]
        [Authorize(Policy = "ReviewWritePolicy")]
        public async Task<ActionResult<ReviewDTO>> RespondToReview(int id, ReviewResponseDTO responseDto)
        {
            try
            {
                // Here you would verify that the authenticated user is authorized 
                // to respond to this review (e.g., they own the business/are the worker)
                // This would typically involve checking claims or making a service call

                var review = await reviewService.RespondToReviewAsync(id, responseDto.Response);
                return Ok(review);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error responding to review");
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "ReviewWritePolicy")]
        public async Task<ActionResult> DeleteReview(int id)
        {
            var clientId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(clientId))
                return Unauthorized();

            var success = await reviewService.DeleteReviewAsync(id, clientId);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }


}

