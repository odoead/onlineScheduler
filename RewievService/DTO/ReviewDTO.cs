namespace ReviewService.DTO
{
    public class ReviewDTO
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public string TargetId { get; set; }
        public string TargetType { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Response { get; set; }
        public DateTime? ResponseDate { get; set; }
    }
}
