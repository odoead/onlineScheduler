using Shared.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingService.Entities
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string WorkerId { get; set; }
        public string ClientId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        public int ProductId { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Created;
        public DateTime StartDateLOC { get; set; }
        public DateTime EndDateLOC { get; set; }




    }
}
