using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScheduleService.Entities
{
    public class ScheduleInterval
    {
        [Key]
        public int Id { get; set; }
        public int WeekDay { get; set; }
        public TimeSpan StartTimeLOC { get; set; }
        public TimeSpan IntervalDuration { get; set; }
        public int IntervalType { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }
        public string EmployeeId { get; set; }
        public List<Booking> Bookings { get; set; } = new List<Booking>();
        public bool HasActiveBooking { get; set; } = false;// check if there is active bookings to prevent interval deletion
    }

    public enum IntervalType
    {
        Work,
        Break
    }

}
