using System.ComponentModel.DataAnnotations;

namespace ChatService.Entities
{

    public class ChatMessage
    {
        [Key]
        public Guid Id { get; set; }
        public string SenderId { get; set; }
        public string RecipientId { get; set; }
        public string CompanyGroupId { get; set; }
        public string Text { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }

}
