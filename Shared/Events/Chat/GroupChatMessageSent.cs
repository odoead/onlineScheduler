namespace Shared.Events.Chat
{
    public class GroupChatMessageSent
    {
        public Guid MessageId { get; set; }
        public string SenderId { get; set; }
        public string GroupId { get; set; }
        public DateTime SentAt { get; set; }
    }
}
