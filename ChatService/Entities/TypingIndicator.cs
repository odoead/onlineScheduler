namespace ChatService.Entities
{
    public class TypingIndicator
    {
        public string UserId { get; set; }
        public string RecipientId { get; set; }
        public string GroupId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
