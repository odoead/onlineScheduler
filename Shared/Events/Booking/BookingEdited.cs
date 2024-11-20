namespace Shared.Events.Booking
{
    public class BookingEdited
    {
        public int BookingId { get; set; }
        public DateTime StartDateLOC { get; set; }
        public DateTime? EndDateLOC { get; set; }
        public int ProductId { get; set; }
        public string WorkerId { get; set; }
    }
}
