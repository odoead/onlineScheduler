namespace Shared.Events.Booking
{
    public class WorkerBookingsRequestResult
    {
        public List<WorkerBooking> Bookings { get; set; }

    }
    public class WorkerBooking
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public DateTime StartDateLOC { get; set; }
        public DateTime EndDateLOC { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string Status { get; set; }

    }
}
