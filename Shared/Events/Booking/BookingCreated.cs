namespace Shared.Events.Booking
{
    public class BookingCreated
    {
        public int BookingId { get; set; }
        public DateTime BookingStartDateLOC { get; set; }
        public DateTime? BookingEndDateLOC { get; set; }
        public DateTime BookingStartDateUTC { get; set; }
        public DateTime? BookingEndDateUTC { get; set; }
        public string BookingsClientId { get; set; }
        public string BookingsWorkerId { get; set; }
        public int BookingProductId { get; set; }
        //public DateTime? NotificationTime { get; set; }

    }
}
