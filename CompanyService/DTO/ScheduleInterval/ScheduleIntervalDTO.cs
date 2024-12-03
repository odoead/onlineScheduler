using CompanyService.Entities;

namespace CompanyService.DTO
{
    public class ScheduleIntervalDTO
    {
        public int WeekDay { get; set; }
        public TimeSpan StartTimeLOC { get; set; }
        public TimeSpan FinishTimeLOC { get; set; }
        public List<BookingDTO> Bookings { get; set; }
        public IntervalType IntervalType { get; set; }
    }
}