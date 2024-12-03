namespace BookingService.DTO
{
    public class AddBookingDTO
    {
        public DateTime BookingTimeLOC { get; set; }
        public TimeSpan? Duration { get; set; } = null;
        public string WorkerId { get; set; }
        public int ProductId { get; set; }
    }
}
