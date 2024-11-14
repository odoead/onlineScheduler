using Shared.Data;

namespace Shared.Messages.Schedule
{
    public class ScheduleIntervalCreated
    {
        public int IntervalId { get; set; }
        public string WorkerId { get; set; }
        public TimeSpan StartTimeLOC { get; set; }
        public TimeSpan Duration { get; set; }
        public int IntervalType { get; set; }
        public DayOfTheWeek WeekDay { get; set; }

    }
}
