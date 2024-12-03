namespace CompanyService.Interfaces
{
    public interface IBookingValidationService
    {
        Task<bool> HasActiveBookingsWorker(string workerId);
        Task<bool> HasActiveBookingsCompany(int companyId);
        Task<bool> HasActiveBookingsScheduleInterval(int scheduleIntervalId);
        Task<bool> HasActiveBookingsProduct(int productId);
        Task<bool> IsValidBookingTime(DateTime startDateLoc, DateTime endDateLoc, int companyId, string workerId);
        Task<bool> HasOverlappingBookings(string workerId, DateTime startDateLoc, DateTime endDateLoc);
        Task<bool> HasOverlappingBookings(string workerId, DateTime startDateLoc, DateTime endDateLoc, int? excludeBookingId = null);

    }
}
