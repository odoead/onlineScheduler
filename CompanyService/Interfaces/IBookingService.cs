using CompanyService.DTO.Booking;

namespace CompanyService.Interfaces
{
    public interface IBookingService
    {
        public Task<List<GetBookingDTO>> GetBookingsAsync(string workerEmail);
    }
}
