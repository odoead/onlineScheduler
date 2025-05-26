using System.ComponentModel.DataAnnotations;

namespace ReviewService.DTO
{
    public class SubmitReviewDTO
    {
        public string TargetId { get; set; }
        public string TargetType { get; set; } // WORKER/ PRODUCT
        public string Comment { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }
    }

}
