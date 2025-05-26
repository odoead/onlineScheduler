using ReviewService.DTO;

namespace ReviewService.Interface
{
    public interface IReviewService
    {
        Task<ReviewDTO> GetReviewByIdAsync(int id);
        Task<IEnumerable<ReviewDTO>> GetReviewsByClientIdAsync(string clientId);
        Task<(IEnumerable<ReviewDTO> Reviews, int TotalCount)> GetFilteredReviewsAsync(ReviewFilterDTO filter);
        Task<ReviewSummaryDTO> GetReviewSummaryAsync(string targetId, string targetType);
        Task<ReviewDTO> SubmitReviewAsync(string clientId, SubmitReviewDTO reviewDto);
        Task<ReviewDTO> RespondToReviewAsync(int reviewId, string response);
        Task<bool> DeleteReviewAsync(int id, string clientId);
        Task<bool> CanClientReviewTargetAsync(string clientId, string targetId, string targetType);

    }
}




