using MassTransit;
using Microsoft.EntityFrameworkCore;
using ScheduleService.DB;
using ScheduleService.DTO;
using ScheduleService.Entities;
using ScheduleService.Interfaces;
using Shared.Data;
using Shared.Exceptions.custom_exceptions;
using Shared.Messages.Schedule;

namespace ScheduleService.Services
{
    public class ScheduleServ : IScheduleService
    {
        private readonly Context dbcontext;
        private readonly IPublishEndpoint _publishEndpoint;
        // private readonly ISendEndpoint

        private async Task<bool> IsOverlappingIntervalAsync(TimeSpan newStartTime, TimeSpan newIntervalDuration, int weekDay, string employeeId)
        {
            var newIntervalEnd = newStartTime + newIntervalDuration;
            return await dbcontext.ScheduleIntervals
                .Where(i => i.EmployeeId == employeeId && i.WeekDay == weekDay)
                .AnyAsync(i =>
                    (newStartTime >= i.StartTimeLOC && newStartTime < i.StartTimeLOC + i.IntervalDuration) ||       // интервал начинается внутри существующего
                    (newIntervalEnd > i.StartTimeLOC && newIntervalEnd <= i.StartTimeLOC + i.IntervalDuration) ||   // интервал заканчивается внутри существующего
                    (newStartTime <= i.StartTimeLOC && newIntervalEnd >= i.StartTimeLOC + i.IntervalDuration) ||    // интервал полностью охватывает существующий
                    (newStartTime >= i.StartTimeLOC && newIntervalEnd <= i.StartTimeLOC + i.IntervalDuration)       // интервал полностью внутри существующего
                );
        }

        public async Task<int> AddIntervalAsync(AddScheduleIntervalDTO interval)
        {

            if (await IsOverlappingIntervalAsync(interval.StartTimeLOC, interval.IntervalDuration, interval.WeekDay, interval.EmployeeId))
            {
                throw new BadRequestException("This interval overlaps with an existing interval.");
            }

            var scheduleInterval = new ScheduleInterval
            {
                WeekDay = ((int)(DayOfTheWeek)interval.WeekDay),
                StartTimeLOC = interval.StartTimeLOC,
                IntervalDuration = interval.IntervalDuration,
                IntervalType = ((int)(IntervalType)interval.IntervalType),
                EmployeeId = interval.EmployeeId,
            };

            dbcontext.ScheduleIntervals.Add(scheduleInterval);
            await dbcontext.SaveChangesAsync();

            return scheduleInterval.Id;
        }

        public async Task<bool> UpdateIntervalAsync(UpdateScheduleIntervalDTO updateInterval)
        {
            var interval = await dbcontext.ScheduleIntervals.Include(q => q.Bookings)
                .Where(q => q.Id == updateInterval.Id && q.Bookings.Select(b => b.bookingStatus == (int)BookingStatus.Created).Any()).FirstOrDefaultAsync();
            if (interval == null) return false;



            if (await IsOverlappingIntervalAsync(interval.StartTimeLOC, interval.IntervalDuration, interval.WeekDay, interval.EmployeeId))
            {
                throw new BadRequestException("The interval overlaps with an existing interval.");
            }
            interval.StartTimeLOC = updateInterval.StartTimeLOC;
            interval.IntervalDuration = updateInterval.IntervalDuration;


            dbcontext.ScheduleIntervals.Update(interval);
            await dbcontext.SaveChangesAsync();

            await _publishEndpoint.Publish(new ScheduleIntervalUpdated
            {
                IntervalId = updateInterval.Id,
                StartTimeLOC = updateInterval.StartTimeLOC,
                Duration = updateInterval.IntervalDuration,
                IntervalType = (int)interval.IntervalType,
            });

            return true;
        }

        public async Task<bool> DeleteIntervalAsync(int intervalId)
        {
            var interval = await dbcontext.ScheduleIntervals.Include(q => q.Bookings)
               .Where(q => q.Id == intervalId && q.Bookings.Select(b => b.bookingStatus == (int)BookingStatus.Created).Any()).FirstOrDefaultAsync();
            if (interval == null) return false;

            dbcontext.ScheduleIntervals.Remove(interval);
            await dbcontext.SaveChangesAsync();

            await _publishEndpoint.Publish(new ScheduleIntervalDeleted
            {
                IntervalId = intervalId,
            });

            return true;
        }





        public async Task<List<ScheduleInterval>> GetWeeklyScheduleWithBookingsAsync(string employeeId, DateTime currentDateLOC)
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
                .Where(si => si.EmployeeId == employeeId)
                .OrderBy(si => si.WeekDay)
                .ThenBy(si => si.StartTimeLOC)
                .ToListAsync();

            return scheduleIntervals;
        }





        public async Task<List<ScheduleEmptyWindow>> GetEmptyScheduleTimeByDate(string employeeId, DateTime date)
        {
            // Получаем день недели из переданной даты
            int dayOfWeek = (int)date.DayOfWeek;

            var scheduleIntervals = await dbcontext.ScheduleIntervals
        .Include(q => q.Bookings)
        .Where(q => q.EmployeeId == employeeId &&
                    (int)q.WeekDay == dayOfWeek &&
                    q.IntervalType == (int)IntervalType.Work)
        .OrderBy(q => q.StartTimeLOC)
        .ToListAsync();

            var availableSlots = new List<ScheduleEmptyWindow>();

            foreach (var interval in scheduleIntervals)
            {
                var intervalStart = interval.StartTimeLOC;
                var intervalEnd = interval.StartTimeLOC + interval.IntervalDuration;

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
    }
}
