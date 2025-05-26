namespace Shared.Events.Booking
{
    public class WorkerBookingsRequestResult
    {
        public List<WorkerBooking_ScheduleService> Bookings { get; set; }

    }
    public class WorkerBooking_ScheduleService
    {
        public int BookingId { get; set; }
        public int ProductId { get; set; }
        public DateTime StartDateLOC { get; set; }
        public DateTime EndDateLOC { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string Status { get; set; }

    }
}
