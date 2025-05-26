using System.ComponentModel.DataAnnotations;

namespace ChatService.Entities
{
    public class UserConnection
    {
        [Key]
        public string ConnectionId { get; set; }
        public string UserId { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastActive { get; set; }
    }
}
