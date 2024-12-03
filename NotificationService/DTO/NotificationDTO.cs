namespace NotificationService.DTO
{
    public class NotificationDTO
    {
        public int Id { get; set; }
        public string RecieverId { get; set; }
        public string Service { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> NotificationKeyValues { get; set; }
    }
}
