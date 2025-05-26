namespace Shared.Events.Review
{
    public class ReviewCreated
    {
        public int ReviewId { get; set; }
        public string ClientId { get; set; }
        public string TargetId { get; set; }
        public string TargetType { get; set; }
        public int Rating { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
