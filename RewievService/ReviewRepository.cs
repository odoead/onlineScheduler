using Microsoft.EntityFrameworkCore;
using ReviewService.DTO;
using ReviewService.Interface;
using RewievService;

namespace ReviewService
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly Context dbcontext;

        public ReviewRepository(Context context)
        {
            dbcontext = context;
        }

        public async Task<Review> GetByIdAsync(int id)
        {
            return await dbcontext.Reviews.FindAsync(id);
        }

        public async Task<IEnumerable<Review>> GetByClientIdAsync(string clientId)
        {
            return await dbcontext.Reviews
                .Where(r => r.ClientId == clientId)
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByTargetAsync(string targetId, string targetType)
        {
            return await dbcontext.Reviews
                .Where(r => !r.IsDeleted)
                .Where(r => r.TargetId == targetId && r.TargetType == targetType)
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Review> Reviews, int TotalCount)> GetFilteredReviewsAsync(ReviewFilterDTO filter)
        {
            var query = dbcontext.Reviews.Where(r => !r.IsDeleted).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.TargetId))
                query = query.Where(r => r.TargetId == filter.TargetId);

            if (!string.IsNullOrEmpty(filter.TargetType))
                query = query.Where(r => r.TargetType == filter.TargetType);

            if (filter.MinRating.HasValue)
                query = query.Where(r => r.Rating >= filter.MinRating.Value);

            if (filter.MaxRating.HasValue)
                query = query.Where(r => r.Rating <= filter.MaxRating.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(r => r.SubmittedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(r => r.SubmittedAt <= filter.ToDate.Value);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var reviews = await query
                .OrderByDescending(r => r.SubmittedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (reviews, totalCount);
        }

        public async Task<ReviewSummaryDTO> GetReviewSummaryAsync(string targetId, string targetType)
        {
            var reviews = await dbcontext.Reviews.Where(r => !r.IsDeleted).Where(r => r.TargetId == targetId && r.TargetType == targetType)
                .ToListAsync();

            if (reviews == null || !reviews.Any())
                return new ReviewSummaryDTO
                {
                    TargetId = targetId,
                    TargetType = targetType,
                    AverageRating = 0,
                    ReviewCount = 0,
                    RatingDistribution = new Dictionary<int, int>
                    {
                        { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
                    }
                };

            var distribution = new Dictionary<int, int>
            {
                { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
            };

            foreach (var review in reviews)
            {
                if (distribution.ContainsKey(review.Rating))
                    distribution[review.Rating]++;
            }

            return new ReviewSummaryDTO
            {
                TargetId = targetId,
                TargetType = targetType,
                AverageRating = reviews.Average(r => r.Rating),
                ReviewCount = reviews.Count,
                RatingDistribution = distribution
            };
        }

        public async Task<Review> AddAsync(Review review)
        {
            await dbcontext.Reviews.AddAsync(review);
            await dbcontext.SaveChangesAsync();
            return review;
        }

        public async Task<Review> UpdateAsync(Review review)
        {
            dbcontext.Reviews.Update(review);
            await dbcontext.SaveChangesAsync();
            return review;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var review = await dbcontext.Reviews.FindAsync(id);
            if (review == null)
                return false;

            review.IsDeleted = true;
            review.LastModifiedAt = DateTime.UtcNow;
            dbcontext.Reviews.Update(review);
            return await dbcontext.SaveChangesAsync() > 0;
        }

        public async Task<bool> HasClientReviewedTargetAsync(string clientId, string targetId, string targetType)
        {
            return await dbcontext.Reviews
                .AnyAsync(r => r.ClientId == clientId && r.TargetId == targetId && r.TargetType == targetType && !r.IsDeleted);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await dbcontext.SaveChangesAsync() > 0;
        }
    }
}
