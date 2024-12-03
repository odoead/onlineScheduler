using CompanyService.DTO;
using CompanyService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CompanyService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleIntervalController : ControllerBase
    {
        private readonly IScheduleService scheduleService;
        public ScheduleIntervalController(IScheduleService scheduleService)
        {
            this.scheduleService = scheduleService;
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddInterval([FromBody] AddScheduleIntervalDTO addScheduleIntervalDTO)
        {

            var intervalId = await scheduleService.AddIntervalAsync(
                addScheduleIntervalDTO.WeekDay,
                addScheduleIntervalDTO.StartTimeLOC,
                addScheduleIntervalDTO.FinishTimeLOC,
                addScheduleIntervalDTO.IntervalType,
                addScheduleIntervalDTO.EmployeeId,
                addScheduleIntervalDTO.CompanyId
            );

            return Ok(intervalId);

        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateInterval(int id, [FromBody] UpdateScheduleIntervalDTO updateScheduleIntervalDTO)
        {
            var success = await scheduleService.UpdateIntervalAsync(
                id,
                updateScheduleIntervalDTO.StartTimeLOC,
                updateScheduleIntervalDTO.FinishTimeLOC
            );
            return Ok(success);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteInterval(int id)
        {
            var success = await scheduleService.DeleteIntervalAsync(id);
            return NoContent();
        }

        [HttpGet("weekly/{employeeId}")]
        [Authorize]
        public async Task<IActionResult> GetWeeklySchedule(string employeeId, [FromQuery] DateTime currentDateLOC)
        {
            var schedule = await scheduleService.GetWeeklyScheduleWithBookingsAsync(employeeId, currentDateLOC);
            return Ok(schedule);
        }

        [HttpGet("weekly")]
        [Authorize]
        public async Task<IActionResult> GetWeeklySchedule_Email([FromQuery] DateTime currentDateLOC)
        {
            var emailClaim = User?.FindFirst(ClaimTypes.Email)?.Value;
            if (emailClaim == null)
            {
                return Unauthorized();
            }

            var schedule = await scheduleService.GetWeeklyScheduleWithBookingsAsync_Email(emailClaim, currentDateLOC);
            return Ok(schedule);
        }

        [HttpGet("available/{employeeId}")]
        [Authorize]
        public async Task<IActionResult> GetAvailableSlots(string employeeId, [FromQuery] DateTime date)
        {
            var emptySlots = await scheduleService.GetEmptyScheduleTimeByDate(employeeId, date);
            return Ok(emptySlots);
        }
    }
}

