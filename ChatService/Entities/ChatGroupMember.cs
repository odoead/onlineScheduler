using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatService.Entities
{
    public class ChatGroupMember
    {
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }

        [ForeignKey(nameof(GroupId))]
        public ChatGroup Group { get; set; }
        public string GroupId { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
