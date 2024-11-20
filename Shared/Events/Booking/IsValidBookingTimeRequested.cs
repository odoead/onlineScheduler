namespace Shared.Events.Booking
{
    public class IsValidBookingTimeRequested
    {
        public string WorkerId { get; set; }
        public int ProductId { get; set; }
        public DateTime StartDateLOC { get; set; }
        public DateTime EndDateLOC { get; set; }
    }
}
