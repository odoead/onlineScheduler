using ReviewService.DTO;
using RewievService;

namespace ReviewService.Interface
{
    public interface IReviewRepository
    {
        Task<Review> GetByIdAsync(int id);
        Task<IEnumerable<Review>> GetByClientIdAsync(string clientId);
        Task<IEnumerable<Review>> GetByTargetAsync(string targetId, string targetType);
        Task<(IEnumerable<Review> Reviews, int TotalCount)> GetFilteredReviewsAsync(ReviewFilterDTO filter);
        Task<ReviewSummaryDTO> GetReviewSummaryAsync(string targetId, string targetType);
        Task<Review> AddAsync(Review review);
        Task<Review> UpdateAsync(Review review);
        Task<bool> DeleteAsync(int id);
        Task<bool> HasClientReviewedTargetAsync(string clientId, string targetId, string targetType);
        Task<bool> SaveChangesAsync();
    }
}
