using CompanyService.DTO;

namespace CompanyService.Interfaces
{
    public interface IScheduleService
    {
        public Task<int> AddIntervalAsync(int WeekDay, TimeSpan StartTimeLOC, TimeSpan FinishTimeLOC, int IntervalType, string EmployeeId, int CompanyId);
        public Task<bool> UpdateIntervalAsync(int Id, TimeSpan StartTimeLOC, TimeSpan FinishTimeLOC);
        public Task<bool> DeleteIntervalAsync(int intervalId);
        public Task<List<ScheduleIntervalDTO>> GetWeeklyScheduleWithBookingsAsync(string employeeId, DateTime currentDateLOC);
        public Task<List<ScheduleIntervalDTO>> GetWeeklyScheduleWithBookingsAsync_Email(string Email, DateTime currentDateLOC);

        public Task<List<ScheduleEmptyWindow>> GetEmptyScheduleTimeByDate(string employeeId, DateTime date);
    }
}
