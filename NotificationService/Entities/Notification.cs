using System.ComponentModel.DataAnnotations;

namespace NotificationService.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string RecieverId { get; set; }
        public ServiceType Service { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Data> NotificationKeyValues { get; set; }
    }
}
