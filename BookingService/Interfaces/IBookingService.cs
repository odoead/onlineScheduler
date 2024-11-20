using BookingService.DTO;

namespace BookingService.Interfaces
{
    public interface IBookingService
    {
        public Task AddBookingAsync(AddBookingDTO addBooking);
        public Task EditBookingAsync(EditBookingDTO editBookingDTO);
        public Task ChangeBookingStatusASync(int bookingId, int newStatus);

    }
}
