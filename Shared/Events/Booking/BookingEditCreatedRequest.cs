namespace Shared.Events.Booking
{
    public class BookingEditCreatedRequest
    {
        public int BookingId { get; set; }
        public DateTime StartDateLOC { get; set; }
        public DateTime StartDateUTC { get; set; }
        public DateTime? EndDateLOC { get; set; }
        public int ProductId { get; set; }
        public string WorkerId { get; set; }
        public DateTime EndDateUTC { get; set; }
    }
}
