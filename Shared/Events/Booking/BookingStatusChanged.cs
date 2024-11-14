namespace Shared.Events.Booking
{
    public class BookingStatusChanged
    {

        public int BookingId { get; set; }
        public int NewBookingStatus { get; set; }
    }
}
