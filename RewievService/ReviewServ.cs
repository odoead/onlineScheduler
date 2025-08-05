using MassTransit;
using ReviewService.DTO;
using ReviewService.Interface;
using RewievService;
using Shared.Events.Review;

namespace ReviewService
{
    public class ReviewServ : IReviewService
    {
        private readonly IReviewRepository reviewRepository;
        private readonly IPublishEndpoint publishEndpoint;
        private readonly ILogger<ReviewServ> logger;

        public ReviewServ(IReviewRepository reviewRepository, IPublishEndpoint publishEndpoint, ILogger<ReviewServ> logger)
        {
            this.reviewRepository = reviewRepository;
            this.publishEndpoint = publishEndpoint;
            this.logger = logger;
        }

        public async Task<ReviewDTO> GetReviewByIdAsync(int id)
        {
            var review = await reviewRepository.GetByIdAsync(id);
            if (review == null)
                return null;

            return MapToDto(review);
        }

        public async Task<IEnumerable<ReviewDTO>> GetReviewsByClientIdAsync(string clientId)
        {
            var reviews = await reviewRepository.GetByClientIdAsync(clientId);
            return reviews.Select(r => MapToDto(r));
        }

        public async Task<(IEnumerable<ReviewDTO> Reviews, int TotalCount)> GetFilteredReviewsAsync(ReviewFilterDTO filter)
        {
            var (reviews, totalCount) = await reviewRepository.GetFilteredReviewsAsync(filter);
            var reviewDtos = reviews.Select(r => MapToDto(r));
            return (reviewDtos, totalCount);
        }

        public async Task<ReviewSummaryDTO> GetReviewSummaryAsync(string targetId, string targetType)
        {
            return await reviewRepository.GetReviewSummaryAsync(targetId, targetType);
        }

        public async Task<ReviewDTO> SubmitReviewAsync(string clientId, SubmitReviewDTO reviewDto)
        {
            // Check if client already reviewed this target
            var hasReviewed = await reviewRepository.HasClientReviewedTargetAsync(
                clientId, reviewDto.TargetId, reviewDto.TargetType);

            if (hasReviewed)
            {
                throw new InvalidOperationException("You have already reviewed this target.");
            }

            // Create and save the review
            var review = new Review
            {
                ClientId = clientId,
                TargetId = reviewDto.TargetId,
                TargetType = reviewDto.TargetType,
                Comment = reviewDto.Comment,
                Rating = reviewDto.Rating,
                SubmittedAt = DateTime.UtcNow
            };

            var savedReview = await reviewRepository.AddAsync(review);

            // Publish event that a review was created
            await publishEndpoint.Publish(new ReviewCreated
            {
                ReviewId = savedReview.Id,
                ClientId = savedReview.ClientId,
                TargetId = savedReview.TargetId,
                TargetType = savedReview.TargetType,
                Rating = savedReview.Rating,
                SubmittedAt = savedReview.SubmittedAt
            });

            return MapToDto(savedReview);
        }

        public async Task<ReviewDTO> RespondToReviewAsync(int reviewId, string response)
        {
            var review = await reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
                throw new KeyNotFoundException("Review not found");

            review.Response = response;
            review.ResponseDate = DateTime.UtcNow;
            review.LastModifiedAt = DateTime.UtcNow;

            var updatedReview = await reviewRepository.UpdateAsync(review);

            // Publish event that a review response was added
            await publishEndpoint.Publish(new ReviewResponded
            {
                ReviewId = updatedReview.Id,
                TargetId = updatedReview.TargetId,
                TargetType = updatedReview.TargetType,
                Response = updatedReview.Response,
                ResponseDate = updatedReview.ResponseDate.Value
            });

            return MapToDto(updatedReview);
        }

        public async Task<bool> DeleteReviewAsync(int id, string clientId)
        {
            var review = await reviewRepository.GetByIdAsync(id);
            if (review == null || review.ClientId != clientId)
            { return false; }
            if (review.IsDeleted)
            { return true; }
            return await reviewRepository.DeleteAsync(id);
        }

        public async Task<bool> CanClientReviewTargetAsync(string clientId, string targetId, string targetType)
        {
            var hasActiveReview = await reviewRepository.HasClientReviewedTargetAsync(clientId, targetId, targetType);
            return !hasActiveReview;
        }

        private ReviewDTO MapToDto(Review review)
        {
            return new ReviewDTO
            {
                Id = review.Id,
                ClientId = review.ClientId,
                TargetId = review.TargetId,
                TargetType = review.TargetType,
                Comment = review.Comment,
                Rating = review.Rating,
                SubmittedAt = review.SubmittedAt,
                Response = review.Response,
                ResponseDate = review.ResponseDate
            };
        }
    }
}
