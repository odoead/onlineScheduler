using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyService.Entities
{
    public class ScheduleInterval
    {
        [Key]
        public int Id { get; set; }
        public int WeekDay { get; set; }
        public TimeSpan StartTimeLOC { get; set; }
        public TimeSpan FinishTimeLOC { get; set; }
        public IntervalType IntervalType { get; set; } = IntervalType.Work;

        [ForeignKey("WorkerId")]
        public Worker Worker { get; set; }
        public string WorkerId { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
        public int CompanyId { get; set; }
        public List<Booking> Bookings { get; set; }
    }

    public enum IntervalType
    {
        Work,
        Break
    }
}
