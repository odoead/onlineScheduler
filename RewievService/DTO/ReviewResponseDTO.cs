using System.ComponentModel.DataAnnotations;

namespace ReviewService.DTO
{
    public class ReviewResponseDTO
    {
        [Required]
        public int ReviewId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Response { get; set; }
    }
}
