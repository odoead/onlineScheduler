namespace Shared.Events.Booking
{
    public class BookingConfirmed
    {
        public int BookingId { get; init; }
        public string WorkerId { get; init; }
        public int ProductId { get; init; }
        public DateTime BookingStartDateLOC { get; init; }
        public DateTime BookingStartDateUTC { get; init; }

        public DateTime? BookingEndDateLOC { get; init; }
    }
}
