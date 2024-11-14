namespace BookingService.DTO
{
    public class EditBookingDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public DateTime BookingTimeLOC { get; set; }
        public string WorkerId { get; set; }
    }
}
