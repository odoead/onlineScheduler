using Shared.Data;

namespace Shared.Events.Booking
{
    public class BookingCanceled
    {
        public int BookingId { get; init; }
        public string WorkerId { get; init; }
        public int ProductId { get; init; }
        public BookingStatus OriginalStatus { get; init; }
        public DateTime StartDateLOC { get; set; }
        public DateTime? EndDateLOC { get; set; }

        public DateTime StartDateUTC { get; set; }
        public DateTime? EndDateUTC { get; set; }
    }
}
