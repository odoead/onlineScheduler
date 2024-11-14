namespace Shared.Messages.Schedule
{
    public class ScheduleIntervalUpdated
    {
        public int IntervalId { get; set; }
        public TimeSpan StartTimeLOC { get; set; }
        public TimeSpan Duration { get; set; }
        public int IntervalType { get; set; }
    }
}
