using CompanyService.Entities;
using ScheduleService.DTO;

namespace CompanyService.Interfaces
{
    public interface IScheduleService
    {
        public Task<int> AddIntervalAsync(AddScheduleIntervalDTO interval);
        public Task<bool> UpdateIntervalAsync(UpdateScheduleIntervalDTO updateInterval);

        public Task<bool> DeleteIntervalAsync(int intervalId);
        public Task<List<ScheduleInterval>> GetWeeklyScheduleWithBookingsAsync(string employeeId, DateTime currentDateLOC);
        public Task<List<ScheduleEmptyWindow>> GetEmptyScheduleTimeByDate(string employeeId, DateTime date);
    }
}
