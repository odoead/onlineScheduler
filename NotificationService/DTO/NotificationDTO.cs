namespace NotificationService.DTO
{
    public class NotificationDTO
    {
        public int Id { get; set; }
        public string RecieverId { get; set; }
        public string Service { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAtUTC { get; set; }
        public DateTime? DeliveredAtUTC { get; set; }
        public DateTime? ReadAtUTC { get; set; }
        public Dictionary<string, string> NotificationKeyValues { get; set; }
    }
}
