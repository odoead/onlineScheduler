using CompanyService.DTO.Booking;

namespace CompanyService.Interfaces
{
    public interface IBookingService
    {
        public Task<List<GetBookingDTO_Worker>> GetWorkerBookingsAsync(string workerEmail);
        public Task<List<GetBookingDTO_Client>> GetClientBookingsAsync(string clientEmail);
        Task<BookingStatisticsDTO> GetCompanyBookingsStatisticsAsync(int companyId, DateTime? startDate, DateTime? endDate);
        ///TODO
    }
}
