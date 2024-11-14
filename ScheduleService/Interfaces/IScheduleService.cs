using ScheduleService.DTO;
using ScheduleService.Entities;

namespace ScheduleService.Interfaces
{
    public interface IScheduleService
    {
        public Task<int> AddIntervalAsync(AddScheduleIntervalDTO interval);
        public Task<bool> UpdateIntervalAsync(UpdateScheduleIntervalDTO updateInterval);
        public Task<bool> DeleteIntervalAsync(int intervalId);
        public Task<List<ScheduleInterval>> GetScheduleByDayAsync(string workerId, int dayOfWeek);

    }
}
