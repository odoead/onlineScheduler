using System.ComponentModel.DataAnnotations.Schema;

namespace ScheduleService.Entities
{
    public class Booking
    {
        public int Id { get; set; }
        public int BookerName { get; set; }
        public DateTime StartDateLOC { get; set; }
        public DateTime EndDateLOC { get; set; }
        public string ProductTitle { get; set; }
        [ForeignKey("ScheduleIntervalId")]
        public ScheduleInterval ScheduleInterval { get; set; }
        public int ScheduleIntervalId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }
        public string EmployeeId { get; set; }
        public int bookingStatus { get; set; }

    }
}
