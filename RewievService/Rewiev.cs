using System.ComponentModel.DataAnnotations;

namespace RewievService
{
    public class Review
    {
        [Key] public int Id { get; set; }
        public string ClientId { get; set; }
        public string TargetId { get; set; }
        public string TargetType { get; set; }
        public string Comment { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }

        public DateTime SubmittedAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? LastModifiedAt { get; set; }

        /*// Optional response from business owner/worker*/
        public string Response { get; set; }
        public DateTime? ResponseDate { get; set; }
    }

}
