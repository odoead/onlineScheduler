namespace ReviewService.DTO
{
    public class ReviewSummaryDTO
    {
        public string TargetId { get; set; }
        public string TargetType { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } // Key: rating (1-5), Value: count

    }
}
