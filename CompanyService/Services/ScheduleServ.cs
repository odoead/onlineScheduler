using CompanyService.DB;
using CompanyService.DTO;
using CompanyService.Entities;
using CompanyService.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.Events.User;
using Shared.Exceptions.custom_exceptions;
using Shared.Messages.Schedule;

namespace CompanyService.Services
{
    public class ScheduleServ : IScheduleService
    {
        private readonly Context dbcontext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IBookingValidationService bookingValidation;
        private readonly IRequestClient<UserEmailRequested> UserEmailclient;

        public ScheduleServ(Context context, IPublishEndpoint publishEndpoint, IBookingValidationService validationService, IRequestClient<UserEmailRequested> client)
        {
            dbcontext = context;
            _publishEndpoint = publishEndpoint;
            bookingValidation = validationService;
            UserEmailclient = client;
        }
        public async Task<int> AddIntervalAsync(int WeekDay, TimeSpan StartTimeLOC, TimeSpan FinishTimeLOC, int IntervalType, string EmployeeId, int CompanyId)
        {

            if (await IsOverlappingIntervalAsync(StartTimeLOC, FinishTimeLOC, WeekDay, EmployeeId))
            {
                throw new BadRequestException("This interval overlaps with an existing interval.");
            }
            if (await IsIntervalInCompanyWorkHoursRange(CompanyId, StartTimeLOC, FinishTimeLOC))
            {
                throw new BadRequestException("Interval is out of the company working hours bounds ");
            }

            var scheduleInterval = new ScheduleInterval
            {
                WeekDay = (DayOfTheWeek)WeekDay,
                StartTimeLOC = StartTimeLOC,
                FinishTimeLOC = FinishTimeLOC,
                IntervalType = ((IntervalType)IntervalType),
                WorkerId = EmployeeId,
                CompanyId = CompanyId,
            };

            dbcontext.ScheduleIntervals.Add(scheduleInterval);
            await dbcontext.SaveChangesAsync();

            return scheduleInterval.Id;
        }

        public async Task<bool> UpdateIntervalAsync(int Id, TimeSpan StartTimeLOC, TimeSpan FinishTimeLOC)
        {
            var interval = await dbcontext.ScheduleIntervals.Include(q => q.Company).Include(q => q.Bookings).Include(q => q.Worker).ThenInclude(q => q.CompanyWorkAssignments).ThenInclude(q => q.Company)
                .Where(q => q.Id == Id).FirstOrDefaultAsync();
            if (interval == null) return false;

            var UTCOffset = interval.Company.TimeZoneFromUTCOffset;

            var hasActiveBookings = interval.Bookings.Any(b => b.EndDateLOC >= DateTime.UtcNow + UTCOffset);

            if (hasActiveBookings)
                throw new BadRequestException("Cannot update interval with active or future bookings.");

            if (await IsOverlappingIntervalAsync(interval.StartTimeLOC, interval.FinishTimeLOC, (int)interval.WeekDay, interval.WorkerId))
            {
                throw new BadRequestException("The interval overlaps with an existing interval.");
            }

            if (await IsIntervalInCompanyWorkHoursRange(interval.CompanyId, interval.StartTimeLOC, interval.FinishTimeLOC))
            {
                throw new BadRequestException("Interval is out of the company working hours bounds ");
            }

            interval.StartTimeLOC = StartTimeLOC;
            interval.FinishTimeLOC = FinishTimeLOC;

            dbcontext.ScheduleIntervals.Update(interval);
            await dbcontext.SaveChangesAsync();

            /*await _publishEndpoint.Publish(new ScheduleIntervalUpdated
            {
                IntervalId = updateInterval.Id,
                StartTimeLOC = updateInterval.StartTimeLOC,
                Duration = updateInterval.IntervalDuration,
                IntervalType = (int)interval.IntervalType,
            });*/

            return true;
        }

        public async Task<bool> DeleteIntervalAsync(int intervalId)
        {
            var interval = await dbcontext.ScheduleIntervals
            .FirstOrDefaultAsync(q => q.Id == intervalId);

            if (interval == null)
                throw new NotFoundException("Schedule interval not found");

            if (await bookingValidation.HasActiveBookingsScheduleInterval(intervalId))
                throw new BadRequestException("Cannot delete interval with active bookings");

            dbcontext.ScheduleIntervals.Remove(interval);
            await dbcontext.SaveChangesAsync();
            await _publishEndpoint.Publish(new ScheduleIntervalDeleted
            {
                IntervalId = intervalId,
            });
            return true;
        }

        public async Task<List<ScheduleIntervalDTO>> GetWeeklyScheduleWithBookingsAsync(string employeeId, DateTime currentDateLOC)
        {
            var currentWeekStart = currentDateLOC.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);

            // If today is Sunday, the currentWeekStart will go back to the previous Monday
            if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
            {
                currentWeekStart = currentWeekStart.AddDays(-7);
            }
            var currentWeekEnd = currentWeekStart.AddDays(7);

            var scheduleIntervals = await dbcontext.ScheduleIntervals
                .Include(si => si.Bookings)
                .Where(si => si.WorkerId == employeeId)
                .OrderBy(si => si.WeekDay)
                .ThenBy(si => si.StartTimeLOC)
                .ToListAsync();

            var result = new List<ScheduleIntervalDTO> { };
            foreach (var q in scheduleIntervals)
            {
                var dto = new ScheduleIntervalDTO
                {
                    FinishTimeLOC = q.FinishTimeLOC,
                    StartTimeLOC = q.StartTimeLOC,
                    WeekDay = (int)q.WeekDay,
                    Bookings = q.Bookings?.Select(b => new BookingDTO
                    {
                        EndDateLOC = b.EndDateLOC,
                        ProductId = b.ProductId,
                        ProductName = b.Product.Name,
                        StartDateLOC = b.StartDateLOC,
                    }).ToList() ?? new List<BookingDTO>(),
                    IntervalType = q.IntervalType,
                };
                result.Add(dto);
            }

            return result;
        }

        public async Task<List<ScheduleEmptyWindow>> GetEmptyScheduleTimeByDate(string employeeId, DateTime date)
        {
            // Получаем день недели из переданной даты
            int dayOfWeek = (int)date.DayOfWeek;

            var scheduleIntervals = await dbcontext.ScheduleIntervals.Include(q => q.Bookings)
                .Where(q => q.WorkerId == employeeId && (int)q.WeekDay == dayOfWeek && q.IntervalType == (int)IntervalType.WORK)
                .OrderBy(q => q.StartTimeLOC).ToListAsync();

            var availableSlots = new List<ScheduleEmptyWindow>();
            foreach (var interval in scheduleIntervals)
            {
                var intervalStart = interval.StartTimeLOC;
                var intervalEnd = interval.FinishTimeLOC;

                // Получаем все бронирования на указанную дату
                var dayBookings = interval.Bookings
                    .Where(b => b.StartDateLOC.Date == date.Date)
                    .OrderBy(b => b.StartDateLOC.TimeOfDay)
                    .ToList();

                if (!dayBookings.Any())
                {
                    availableSlots.Add(new ScheduleEmptyWindow
                    {
                        BeginTime = intervalStart,
                        EndTime = intervalEnd,
                        Duration = intervalEnd - intervalStart
                    });
                    continue;
                }

                // Проверяем свободное время до первого бронирования
                var firstBooking = dayBookings.First();
                if (intervalStart < firstBooking.StartDateLOC.TimeOfDay)
                {
                    availableSlots.Add(new ScheduleEmptyWindow
                    {
                        BeginTime = intervalStart,
                        EndTime = firstBooking.StartDateLOC.TimeOfDay,
                        Duration = firstBooking.StartDateLOC.TimeOfDay - intervalStart
                    });
                }

                // Проверяем промежутки между бронированиями
                for (int i = 0; i < dayBookings.Count - 1; i++)
                {
                    var currentBookingEnd = dayBookings[i].EndDateLOC.TimeOfDay;
                    var nextBookingStart = dayBookings[i + 1].StartDateLOC.TimeOfDay;

                    if (currentBookingEnd < nextBookingStart)
                    {
                        availableSlots.Add(new ScheduleEmptyWindow
                        {
                            BeginTime = currentBookingEnd,
                            EndTime = nextBookingStart,
                            Duration = nextBookingStart - currentBookingEnd
                        });
                    }
                }

                // Проверяем свободное время после последнего бронирования
                var lastBooking = dayBookings.Last();
                if (lastBooking.EndDateLOC.TimeOfDay < intervalEnd)
                {
                    availableSlots.Add(new ScheduleEmptyWindow
                    {
                        BeginTime = lastBooking.EndDateLOC.TimeOfDay,
                        EndTime = intervalEnd,
                        Duration = intervalEnd - lastBooking.EndDateLOC.TimeOfDay
                    });
                }
            }

            return availableSlots
                .Where(slot => slot.Duration > TimeSpan.Zero)
                .OrderBy(slot => slot.BeginTime)
                .ToList();
        }
        private async Task<bool> IsIntervalInCompanyWorkHoursRange(int companyId, TimeSpan StartTime, TimeSpan FinishTime)
        {
            var company = await dbcontext.Companies.FirstOrDefaultAsync(q => q.Id == companyId);
            return (StartTime <= company.ClosingTimeLOC && StartTime >= company.OpeningTimeLOC) &&
                (FinishTime <= company.ClosingTimeLOC && FinishTime >= company.OpeningTimeLOC);
        }

        private async Task<bool> IsOverlappingIntervalAsync(TimeSpan StartTime, TimeSpan FinishTime, int weekDay, string employeeId, int? excludeIntervalId = null)
        {
            var intervals = dbcontext.ScheduleIntervals
                .Where(i => i.WorkerId == employeeId && (int)i.WeekDay == weekDay);

            if (excludeIntervalId.HasValue)
                intervals = intervals.Where(i => i.Id != excludeIntervalId.Value);

            return await intervals.AnyAsync(i =>
                    (StartTime >= i.StartTimeLOC && StartTime < i.FinishTimeLOC) ||     // интервал начинается внутри существующего
                    (FinishTime > i.StartTimeLOC && FinishTime <= i.FinishTimeLOC) ||   // интервал заканчивается внутри существующего
                    (StartTime <= i.StartTimeLOC && FinishTime >= i.FinishTimeLOC) ||   // интервал полностью охватывает существующий
                    (StartTime >= i.StartTimeLOC && FinishTime <= i.FinishTimeLOC)      // интервал полностью внутри существующего
            );
        }

        public async Task<List<ScheduleIntervalDTO>> GetWeeklyScheduleWithBookingsAsync_Email(string Email, DateTime currentDateLOC)
        {
            var response = await UserEmailclient.GetResponse<UserEmailRequestResult, UserEmailRequestedNotFoundResult>(new UserEmailRequested { Email = Email });
            string clientId;
            switch (response)
            {
                case var r when r.Message is UserEmailRequestResult result:
                    clientId = result.Id;
                    break;
                case var r when r.Message is UserEmailRequestedNotFoundResult notFoundResult:
                    throw new BadRequestException("User with email " + Email + " not found");

                default:
                    throw new InvalidOperationException("Unknown response type received.");
            }

            return await GetWeeklyScheduleWithBookingsAsync(clientId, currentDateLOC);
        }
    }
}
