using CompanyService.DTO;
using CompanyService.DTO.Worker;

namespace CompanyService.Interfaces
{
    public interface IScheduleService
    {
        public Task<int> AddIntervalAsync(int WeekDay, TimeSpan StartTimeLOC, TimeSpan FinishTimeLOC, int IntervalType, string EmployeeId,
            int CompanyId);
        public Task<bool> UpdateIntervalAsync(int Id, TimeSpan StartTimeLOC, TimeSpan FinishTimeLOC);
        public Task<bool> DeleteIntervalAsync(int intervalId);
        public Task<List<ScheduleIntervalDTO>> GetEmployeeCurrentWeekScheduleWithBookingsAsync(string employeeId, DateTime currentDateLOC);
        public Task<List<ScheduleIntervalDTO>> GetEmployeeCurrentWeekScheduleWithBookingsAsync_Email(string employeeEmail, DateTime currentDateLOC);
        public Task<List<ScheduleEmptyWindow>> GetEmployeeEmptyScheduleTimeByDate(string employeeId, DateTime date);
        public Task<List<List<ScheduleEmptyWindow>>> GetEmployeeCurrentWeekEmptyScheduleTime(string employeeId, DateTime currentDateLOC);
        public Task<List<ScheduleIntervalDTO>> GetEmployeeWeeklyScheduleWithBookingsByWeekDayAsync(string employeeId, DateTime date);
        public Task<List<WorkerEmptySchedulesDTO>> GetCompanyCurrentWeekEmptyScheduleTimeForProduct(int companyId, int productId, DateTime currentDateLOC);
        public Task<List<WorkerScheduleIntervalDTO>> GetCompanyCurrentWeekScheduleWithBookingsAsync(int companyId, DateTime currentDateLOC);
        public Task<List<WorkerEmptySchedulesDTO>> GetCompanyEmptyScheduleTimeByDateForProduct(int companyId, DateTime date, int productId);
        public Task<List<WorkerScheduleIntervalDTO>> GetCompanyWeeklyScheduleWithBookingsByWeekDayAsync(int companyId, DateTime date);
        ///todo
    }
}
