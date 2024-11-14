using Shared.Data;

namespace BookingService.Entities
{
    public class ScheduleInterval
    {
        public int Id { get; set; }
        public DayOfTheWeek WeekDay { get; set; }
        public TimeSpan StartTimeLOC { get; set; }
        public TimeSpan IntervalDuration { get; set; }
    }
}
