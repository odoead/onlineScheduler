namespace Shared.Events.Booking
{
    public class BookingConfirmationRequested
    {
        public int BookingId { get; init; }
        public string WorkerId { get; init; }
        public int ProductId { get; init; }
        public DateTime StartDateLOC { get; init; }
        public DateTime EndDateLOC { get; init; }
    }
}
