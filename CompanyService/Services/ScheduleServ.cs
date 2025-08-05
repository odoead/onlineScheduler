using CompanyService.DB;
using CompanyService.DTO;
using CompanyService.DTO.Worker;
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
        private readonly IPublishEndpoint publishEndpoint;
        private readonly IBookingValidationService bookingValidation;
        private readonly IRequestClient<UserEmailRequested> UserEmailclient;

        public ScheduleServ(Context context, IPublishEndpoint publishEndpoint, IBookingValidationService validationService, IRequestClient<UserEmailRequested> client)
        {
            dbcontext = context;
            this.publishEndpoint = publishEndpoint;
            bookingValidation = validationService;
            UserEmailclient = client;
        }

        public async Task<int> AddIntervalAsync(int WeekDay, TimeSpan StartTimeLOC, TimeSpan FinishTimeLOC, int IntervalType, string EmployeeId, int CompanyId)
        {

            if (await IsOverlappingIntervalAsync(StartTimeLOC, FinishTimeLOC, WeekDay, EmployeeId))
            {
                throw new BadRequestException("This interval overlaps with an existing interval.");
            }
            if (!await IsIntervalInCompanyWorkHoursRange(CompanyId, StartTimeLOC, FinishTimeLOC))
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
            var interval = await dbcontext.ScheduleIntervals.Include(q => q.Company).Include(q => q.Bookings).Include(q => q.Worker)
                .ThenInclude(q => q.CompanyWorkAssignments).ThenInclude(q => q.Company)
                .Where(q => q.Id == Id).FirstOrDefaultAsync();
            if (interval == null) return false;

            var UTCOffset = interval.Company.TimeZoneFromUTCOffset;

            var hasActiveBookings = interval.Bookings.Any(b => b.EndDateLOC >= DateTime.UtcNow + UTCOffset);

            if (hasActiveBookings)
                throw new BadRequestException("Cannot update interval with active or future bookings.");

            if (await IsOverlappingIntervalAsync(interval.StartTimeLOC, interval.FinishTimeLOC, (int)interval.WeekDay, interval.WorkerId, Id))
            {
                throw new BadRequestException("The interval overlaps with an existing interval.");
            }

            if (!await IsIntervalInCompanyWorkHoursRange(interval.CompanyId, interval.StartTimeLOC, interval.FinishTimeLOC))
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
            await publishEndpoint.Publish(new ScheduleIntervalDeleted
            {
                IntervalId = intervalId,
            });
            return true;
        }


        public async Task<List<ScheduleIntervalDTO>> GetEmployeeCurrentWeekScheduleWithBookingsAsync(string employeeId, DateTime currentDateLOC)
        {
            var currentWeekStart = GetWeeksMondayByWeekPassedDate(currentDateLOC);
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

        //
        public async Task<List<ScheduleEmptyWindow>> GetEmployeeEmptyScheduleTimeByDate(string employeeId, DateTime date)
        {
            int dayOfWeek = (int)DayOfTheWeekExtensions.ToCustomDayOfWeek(date);

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

            return availableSlots.Where(slot => slot.Duration > TimeSpan.Zero).OrderBy(slot => slot.BeginTime).ToList();
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



        public async Task<List<ScheduleIntervalDTO>> GetEmployeeCurrentWeekScheduleWithBookingsAsync_Email(string Email, DateTime currentDateLOC)
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

            return await GetEmployeeCurrentWeekScheduleWithBookingsAsync(clientId, currentDateLOC);
        }



        //---
        public async Task<List<List<ScheduleEmptyWindow>>> GetEmployeeCurrentWeekEmptyScheduleTime(string employeeId, DateTime currentDateLOC)
        {
            var currentWeekStart = GetWeeksMondayByWeekPassedDate(currentDateLOC);

            var weeklyEmptySchedule = new List<List<ScheduleEmptyWindow>>();
            for (int i = 1; i < 8; i++)
            {
                var day = currentWeekStart.AddDays(i);
                var emptyWindows = await GetEmployeeEmptyScheduleTimeByDate(employeeId, day);
                weeklyEmptySchedule.Add(emptyWindows);
            }

            return weeklyEmptySchedule;
        }

        public async Task<List<ScheduleIntervalDTO>> GetEmployeeWeeklyScheduleWithBookingsByWeekDayAsync(string employeeId, DateTime date)
        {
            // Find the Monday of the week containing the given date
            var weekStart = GetWeeksMondayByWeekPassedDate(date);
            return await GetEmployeeCurrentWeekScheduleWithBookingsAsync(employeeId, weekStart);
        }

        // Get the current week schedule for a company's workers
        public async Task<List<WorkerEmptySchedulesDTO>> GetCompanyCurrentWeekEmptyScheduleTimeForProduct(int companyId, int productId, DateTime currentDateLOC)
        {
            var product = await dbcontext.Products.Include(p => p.AssignedWorkers).ThenInclude(pw => pw.Worker)
                .FirstOrDefaultAsync(p => p.Id == productId && p.CompanyId == companyId) ??
                throw new NotFoundException("Product not found with id " + productId);

            var workerIds = product.AssignedWorkers.Select(pw => pw.WorkerId).ToList();
            if (!workerIds.Any())
                return new List<WorkerEmptySchedulesDTO>();

            var currentDate = currentDateLOC.Date;
            var workersSchedules = new List<WorkerEmptySchedulesDTO>();
            foreach (var workerId in workerIds)
            {
                var workerSlots = await GetEmployeeEmptyScheduleTimeByDate(workerId, currentDate);
                var validSlots = workerSlots.Where(slot => slot.Duration >= product.Duration).ToList();
                workersSchedules.Add(new WorkerEmptySchedulesDTO
                {
                    WorkerId = workerId,
                    WorkerName = product.AssignedWorkers.FirstOrDefault(q => q.WorkerId == workerId)?.Worker.FullName,
                    EmptySchedules = validSlots,
                });

            }
            return workersSchedules;
        }

        public async Task<List<WorkerScheduleIntervalDTO>> GetCompanyCurrentWeekScheduleWithBookingsAsync(int companyId, DateTime currentDateLOC)
        {
            var company = await dbcontext.Companies
                .Include(c => c is SharedCompany ? ((SharedCompany)c).Workers : null)
                .Include(c => c is PersonalCompany ? ((PersonalCompany)c).Worker : null)
                .FirstOrDefaultAsync(c => c.Id == companyId) ??
                throw new NotFoundException("Company not found with id " + companyId);

            List<string> workerIds;
            if (company is SharedCompany sharedCompany)
            {
                workerIds = sharedCompany.Workers.Select(w => w.WorkerId).ToList();
            }
            else if (company is PersonalCompany personalCompany)
            {
                workerIds = new List<string> { personalCompany.WorkerId };
            }
            else
            {
                throw new InvalidOperationException("Unknown company type");
            }

            var scheduleIntervals = await dbcontext.ScheduleIntervals
                .Include(si => si.Bookings)
                .Include(si => si.Worker)
                .Where(si => si.CompanyId == companyId && workerIds.Contains(si.WorkerId))
                .OrderBy(si => si.WeekDay)
                .ThenBy(si => si.StartTimeLOC)
                .ToListAsync();

            var groupedByWorker = scheduleIntervals.GroupBy(si => si.WorkerId)
                .Select(group => new WorkerScheduleIntervalDTO
                {
                    WorkerId = group.Key,
                    WorkerName = group.First().Worker.FullName,
                    EmptySchedules = group.Select(si => new ScheduleIntervalDTO
                    {
                        FinishTimeLOC = si.FinishTimeLOC,
                        StartTimeLOC = si.StartTimeLOC,
                        WeekDay = (int)si.WeekDay,
                        Bookings = si.Bookings?.Select(b => new BookingDTO
                        {
                            EndDateLOC = b.EndDateLOC,
                            ProductId = b.ProductId,
                            ProductName = b.Product.Name,
                            StartDateLOC = b.StartDateLOC
                        }).ToList() ?? new List<BookingDTO>(),
                        IntervalType = si.IntervalType
                    }).ToList()
                }).ToList();

            return groupedByWorker;
        }

        // Find all available slots for each worker on the given date
        public async Task<List<WorkerEmptySchedulesDTO>> GetCompanyEmptyScheduleTimeByDateForProduct(int companyId, DateTime date, int productId)
        {
            var product = await dbcontext.Products.Include(p => p.AssignedWorkers).ThenInclude(pw => pw.Worker).FirstOrDefaultAsync(p => p.Id == productId && p.CompanyId == companyId) ??
                throw new NotFoundException("Product not found with id " + productId);

            var workerIds = product.AssignedWorkers.Select(pw => pw.WorkerId).ToList();
            if (!workerIds.Any())
                return new List<WorkerEmptySchedulesDTO>();

            var workersSchedules = new List<WorkerEmptySchedulesDTO>();
            foreach (var workerId in workerIds)
            {

                var workerSlots = await GetEmployeeEmptyScheduleTimeByDate(workerId, date);
                var validSlots = workerSlots.Where(slot => slot.Duration >= product.Duration).ToList();
                workersSchedules.Add(new WorkerEmptySchedulesDTO
                {
                    WorkerId = workerId,
                    WorkerName = product.AssignedWorkers.FirstOrDefault(q => q.WorkerId == workerId)?.Worker.FullName,
                    EmptySchedules = validSlots
                });

            }
            return workersSchedules;
        }

        public async Task<List<WorkerScheduleIntervalDTO>> GetCompanyWeeklyScheduleWithBookingsByWeekDayAsync(int companyId, DateTime date)
        {
            var weekStart = GetWeeksMondayByWeekPassedDate(date);
            return await GetCompanyCurrentWeekScheduleWithBookingsAsync(companyId, weekStart);
        }

        private DateTime GetWeeksMondayByWeekPassedDate(DateTime currentDateLOC)
        {
            var currentWeekStart = currentDateLOC.AddDays(-(int)currentDateLOC.DayOfWeek + (int)DayOfWeek.Monday);
            if (currentDateLOC.DayOfWeek == DayOfWeek.Sunday)
            {
                currentWeekStart = currentWeekStart.AddDays(-7);
            }
            return currentWeekStart;
        }
    }
}
