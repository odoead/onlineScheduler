namespace Shared.Notification
{
    public class ReminderNotificationCreated
    {
        public int NotificationId { get; set; }
        public string RecieverId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
    }
}
