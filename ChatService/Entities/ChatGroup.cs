using System.ComponentModel.DataAnnotations;

namespace ChatService.Entities
{
    public class ChatGroup
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ChatGroupMember> Members { get; set; } = new List<ChatGroupMember>();
    }
}
