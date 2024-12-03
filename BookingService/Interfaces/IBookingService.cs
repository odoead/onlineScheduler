namespace BookingService.Interfaces
{
    public interface IBookingService
    {
        public Task AddBookingAsync(DateTime BookingTimeLOC, string WorkerId, string ClientEmail, int ProductId, TimeSpan? Duration = null);
        public Task EditBookingAsync(int Id, DateTime BookingTimeLOC, string WorkerId);
        public Task ChangeBookingStatusAsync(int bookingId, int newStatus);

    }
}
