using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotificationService.Entities
{
    public class Data
    {
        [Key]
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        [ForeignKey("NotificationId")]
        public Notification Notification { get; set; }
        public int NotificationId { get; set; }
    }
}
