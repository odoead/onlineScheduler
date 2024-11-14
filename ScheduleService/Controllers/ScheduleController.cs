using Microsoft.AspNetCore.Mvc;
using ScheduleService.DTO;
using ScheduleService.Interfaces;

namespace ScheduleService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : Controller
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpPost]
        public async Task<IActionResult> AddInterval([FromBody] AddScheduleIntervalDTO interval)
        {
            var intervalId = await _scheduleService.AddIntervalAsync(interval);
            return Ok(intervalId);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateInterval([FromBody] UpdateScheduleIntervalDTO interval)
        {
            var result = await _scheduleService.UpdateIntervalAsync(interval);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpDelete("{intervalId:int}")]
        public async Task<IActionResult> DeleteInterval(int intervalId)
        {
            var result = await _scheduleService.DeleteIntervalAsync(intervalId);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpGet("{workerId:int}/{dayOfWeek:int}")]
        public async Task<IActionResult> GetScheduleByDay(string workerId, int dayOfWeek)
        {
            var schedule = await _scheduleService.GetScheduleByDayAsync(workerId, dayOfWeek);
            return Ok(schedule);
        }
    }
}
