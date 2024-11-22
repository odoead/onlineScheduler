using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyService.Entities
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }
        public int BookingServiceId { get; set; }
        [ForeignKey("WorkerId")]
        public Worker Worker { get; set; }
        public string WorkerId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        public int ProductId { get; set; }
        public DateTime StartDateLOC { get; set; }
        public DateTime EndDateLOC { get; set; }

        [ForeignKey("ScheduleIntervalId")]
        public ScheduleInterval ScheduleInterval { get; set; }
        public int ScheduleIntervalId { get; set; }
    }


}
