using NotificationService.Data;
using System.ComponentModel.DataAnnotations;

namespace NotificationService.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string RecieverId { get; set; }
        public ServiceType Service { get; set; }
        public NotificationType Type { get; set; }
        public NotificationStatus Status { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAtUTC { get; set; }
        public DateTime? DeliveredAtUTC { get; set; }
        public DateTime? ReadAtUTC { get; set; }
        public List<KVData> NotificationKeyValues { get; set; }

        //public DateTime? TimeToDeliver { get; set; }
        //todo 
        //startdateloc
        //enddateloc

    }
}
