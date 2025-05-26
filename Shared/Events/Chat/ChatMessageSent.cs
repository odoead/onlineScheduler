namespace Shared.Events.Chat
{
    public class ChatMessageSent
    {
        public Guid MessageId { get; set; }
        public string SenderId { get; set; }
        public string RecipientId { get; set; }
        public DateTime SentAt { get; set; }
    }
}
