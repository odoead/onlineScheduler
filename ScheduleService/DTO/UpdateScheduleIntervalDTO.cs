﻿namespace ScheduleService.DTO
{
    public class UpdateScheduleIntervalDTO
    {
        public int Id { get; set; }
        public TimeSpan StartTimeLOC { get; set; }
        public TimeSpan IntervalDuration { get; set; }
    }
}
