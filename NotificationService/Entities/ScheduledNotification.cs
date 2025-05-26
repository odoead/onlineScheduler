using NotificationService.Data;
using System.ComponentModel.DataAnnotations;

namespace NotificationService.Entities
{
    public class ScheduledNotification
    {
        [Key]
        public int Id { get; set; }
        public string RecieverId { get; set; }
        public int BookingId { get; set; }
        public DateTime ScheduledDateForUTC { get; set; }
        public NotificationType Type { get; set; }
        public bool IsProcessed { get; set; }
        public DateTime CreatedAtUTC { get; set; }
    }
}
