namespace Shared.Events.Booking
{
    public class BookingStatisticsRequest
    {
        public List<string> WorkerIds { get; set; } = new();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
