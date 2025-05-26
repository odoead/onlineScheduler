using System.ComponentModel.DataAnnotations;

namespace ChatService.Entities
{
    public class UnreadMessageCounter
    {
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string ChatWithUserId { get; set; }
        public string GroupId { get; set; }
        public int Count { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
