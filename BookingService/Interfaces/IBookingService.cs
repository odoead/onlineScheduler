using BookingService.DTO;
using Shared.Data;

namespace BookingService.Interfaces
{
    public interface IBookingService
    {
        public Task AddBookingAsync(AddBookingDTO addBooking);
        public Task EditBookingAsync(EditBookingDTO edit);
        public Task ChangeBookingStatusASync(int id, BookingStatus newStatus);
    }
}
