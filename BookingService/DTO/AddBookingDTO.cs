namespace BookingService.DTO
{
    public class AddBookingDTO
    {
        public DateTime BookingTimeLOC { get; set; }
        public int CompanyId { get; set; }
        public string WorkerId { get; set; }
        public string ClientId { get; set; }
        public int ProductId { get; set; }
    }
}
