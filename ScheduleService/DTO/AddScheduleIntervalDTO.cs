﻿namespace ScheduleService.DTO
{
    public class AddScheduleIntervalDTO
    {
        public int WeekDay { get; set; }
        public TimeSpan StartTimeLOC { get; set; }
        public TimeSpan IntervalDuration { get; set; }
        public int IntervalType { get; set; }
        public string EmployeeId { get; set; }
    }
}
