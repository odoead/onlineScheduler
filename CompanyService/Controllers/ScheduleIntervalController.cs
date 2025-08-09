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

        [HttpGet("employee/{employeeId}/week")]
        [Authorize]
        public async Task<IActionResult> GetWeeklySchedule(string employeeId, [FromQuery] DateTime currentDateLOC)
        {
            var schedule = await scheduleService.GetEmployeeCurrentWeekScheduleWithBookingsAsync(employeeId, currentDateLOC);
            return Ok(schedule);
        }

        [HttpGet("employee/week")]
        [Authorize]
        public async Task<IActionResult> GetWeeklySchedule_Email([FromQuery] DateTime currentDateLOC)
        {
            var emailClaim = User?.FindFirst(ClaimTypes.Email)?.Value;
            if (emailClaim == null)
            {
                return Unauthorized();
            }

            var schedule = await scheduleService.GetEmployeeCurrentWeekScheduleWithBookingsAsync_Email(emailClaim, currentDateLOC);
            return Ok(schedule);
        }

        [HttpGet("employee/{employeeId}/available")]
        [Authorize]
        public async Task<IActionResult> GetAvailableSlots(string employeeId, [FromQuery] DateTime date)
        {
            var emptySlots = await scheduleService.GetEmployeeEmptyScheduleTimeByDate(employeeId, date);
            return Ok(emptySlots);
        }

        [HttpGet("employee/{employeeId}/week-empty")]
        [Authorize]
        public async Task<IActionResult> GetEmployeeCurrentWeekEmptyScheduleTime(string employeeId, [FromQuery] DateTime currentDateLOC)
        {
            var schedule = await scheduleService.GetEmployeeCurrentWeekEmptyScheduleTime(employeeId, currentDateLOC);
            return Ok(schedule);
        }

        [HttpGet("employee/{employeeId}/week-by-day")]
        [Authorize]
        public async Task<IActionResult> GetEmployeeWeeklyScheduleWithBookingsByWeekDay(string employeeId, [FromQuery] DateTime date)
        {
            var schedule = await scheduleService.GetEmployeeWeeklyScheduleWithBookingsByWeekDayAsync(employeeId, date);
            return Ok(schedule);
        }

        [HttpGet("company/{companyId}/week")]
        [Authorize]
        public async Task<IActionResult> GetCompanyCurrentWeekScheduleWithBookings(int companyId, [FromQuery] DateTime currentDateLOC)
        {
            var schedule = await scheduleService.GetCompanyCurrentWeekScheduleWithBookingsAsync(companyId, currentDateLOC);
            return Ok(schedule);
        }

        [HttpGet("company/{companyId}/product/{productId}/week-empty")]
        [Authorize]
        public async Task<IActionResult> GetCompanyCurrentWeekEmptyScheduleTimeForProduct(int companyId, int productId, [FromQuery] DateTime currentDateLOC)
        {
            var schedule = await scheduleService.GetCompanyCurrentWeekEmptyScheduleTimeForProduct(companyId, productId, currentDateLOC);
            return Ok(schedule);
        }

        [HttpGet("company/{companyId}/product/{productId}/date-empty")]
        [Authorize]
        public async Task<IActionResult> GetCompanyEmptyScheduleTimeByDateForProduct(int companyId, int productId, [FromQuery] DateTime date)
        {
            var schedule = await scheduleService.GetCompanyEmptyScheduleTimeByDateForProduct(companyId, date, productId);
            return Ok(schedule);
        }

        [HttpGet("company/{companyId}/week-by-day")]
        [Authorize]
        public async Task<IActionResult> GetCompanyWeeklyScheduleWithBookingsByWeekDay(int companyId, [FromQuery] DateTime date)
        {
            var schedule = await scheduleService.GetCompanyWeeklyScheduleWithBookingsByWeekDayAsync(companyId, date);
            return Ok(schedule);
        }
    }
}

