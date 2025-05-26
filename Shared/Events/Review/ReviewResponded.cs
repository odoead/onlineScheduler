namespace Shared.Events.Review
{
    public class ReviewResponded
    {
        public int ReviewId { get; set; }
        public string TargetId { get; set; }
        public string TargetType { get; set; }
        public string Response { get; set; }
        public DateTime ResponseDate { get; set; }
    }
}
