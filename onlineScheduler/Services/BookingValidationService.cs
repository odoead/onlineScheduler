using CompanyService.DB;
using CompanyService.Entities;
using CompanyService.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.Exceptions.custom_exceptions;

namespace CompanyService.Services
{
    public class BookingValidationService : IBookingValidationService
    {
        private readonly Context dbcontext;

        public BookingValidationService(Context dbContext)
        {
            dbcontext = dbContext;
        }

        public async Task<bool> HasActiveBookingsWorker(string workerId)
        {
            var currentDateTime = DateTime.UtcNow;

            return await dbcontext.Bookings
                .Where(b => b.WorkerId == workerId)
                .Include(b => b.Product.Company)
                .Include(b => b.Worker.CompanyWorkAssignments)
                    .ThenInclude(cwa => cwa.Company)
                .AnyAsync(b => IsBookingActive(b, currentDateTime));
        }

        public async Task<bool> HasActiveBookingsCompany(int companyId)
        {
            var currentDateTime = DateTime.UtcNow;

            return await dbcontext.Companies
           .Where(c => c.Id == companyId)
           .SelectMany(c => c.Products)
           .SelectMany(p => p.Bookings)
           .AnyAsync(b => IsBookingActive(b, currentDateTime));
        }

        public async Task<bool> HasActiveBookingsScheduleInterval(int scheduleIntervalId)
        {
            var currentDateTime = DateTime.UtcNow;

            return await dbcontext.ScheduleIntervals
                .Where(si => si.Id == scheduleIntervalId).Select(q => q.Company).SelectMany(c => c.Products).SelectMany(p => p.Bookings)
                .AnyAsync(b => IsBookingActive(b, currentDateTime));
        }

        public async Task<bool> HasActiveBookingsProduct(int productId)
        {
            var currentDateTime = DateTime.UtcNow;

            return await dbcontext.Products
                .Where(p => p.Id == productId).Include(b => b.Company)
                .SelectMany(p => p.Bookings)

                .AnyAsync(b => IsBookingActive(b, currentDateTime));
        }

        public async Task<bool> IsValidBookingTime(DateTime startDateLoc, DateTime endDateLoc, int companyId, string workerId)
        {
            if (startDateLoc >= endDateLoc)
            {
                return false;
                //throw new BadRequestException("Booking start time must be before end time");
            }
            //considering the company's timezone
            var company = await dbcontext.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
            if (company == null)
            {
                throw new BadRequestException($"Company not found with ID: {companyId}");
            }

            var currentTimeInCompanyTz = DateTime.UtcNow.Add(company.TimeZoneFromUTCOffset);

            if (endDateLoc <= currentTimeInCompanyTz)
            {
                return false;
                //throw new BadRequestException("Booking end time must be in the future");
            }

            // Check if the booking day is within company working days
            var bookingDayOfWeek = (int)startDateLoc.DayOfWeek;
            if (!company.WorkingDays.Contains((DayOfTheWeek)bookingDayOfWeek))
            {
                return false;
                //throw new BadRequestException("Booking time is outside of company working days");
            }

            //Check if Worker has schedule intervals during the booking time
            //and interval has free time with no bookings at start and end time
            var scheduleIntervals = await dbcontext.ScheduleIntervals
                .Where(si => si.CompanyId == companyId &&
                    si.WorkerId == workerId &&
                    si.WeekDay == bookingDayOfWeek &&
                    si.IntervalType == IntervalType.Work)
                .ToListAsync();
            if (!scheduleIntervals.Any())
            {
                return false;
                //throw new BadRequestException("Worker has no schedule intervals in this company");
            }

            #region Check if booking time falls within any schedule interval
            var bookingStartTime = startDateLoc.TimeOfDay;
            var bookingEndTime = endDateLoc.TimeOfDay;
            bool isWithinSchedule = false;

            foreach (var interval in scheduleIntervals)
            {
                if ((bookingStartTime >= interval.StartTimeLOC && bookingStartTime < interval.FinishTimeLOC) &&
                    (bookingEndTime > interval.StartTimeLOC && bookingEndTime <= interval.FinishTimeLOC))
                {
                    isWithinSchedule = true;
                    break;
                }
            }

            if (!isWithinSchedule)
            {
                return false;
                // throw new BadRequestException("Booking time is outside of worker's schedule intervals");
            }
            #endregion

            #region Check if booking is within company operating hours
            var companyOpenTime = company.OpeningTimeLOC;
            var companyCloseTime = company.ClosingTimeLOC;

            if (bookingStartTime < companyOpenTime || bookingEndTime > companyCloseTime)
            {
                return false;
                //throw new BadRequestException("Booking time is outside of company operating hours");
            }
            #endregion
            if (await HasOverlappingBookings(workerId, startDateLoc, endDateLoc))
            {
                return false;
                //throw new BadRequestException("Booking has overlapping with other bookings");
            }
            return true;
        }

        public async Task<bool> HasOverlappingBookings(string workerId, DateTime startDateLoc, DateTime endDateLoc)
        {
            return await dbcontext.Bookings
                .Where(b => b.WorkerId == workerId && b.StartDateLOC.Date == startDateLoc.Date)
                .AnyAsync(b =>
                    (startDateLoc >= b.StartDateLOC && startDateLoc < b.EndDateLOC) ||  //новый букинг начинается внутри существующего
                    (endDateLoc > b.StartDateLOC && endDateLoc <= b.EndDateLOC) ||      //новый букинг заканчивается внутри существующего
                    (startDateLoc <= b.StartDateLOC && endDateLoc >= b.EndDateLOC) ||   //новый букинг полностью охватывает существующий
                    (startDateLoc >= b.StartDateLOC && endDateLoc <= b.EndDateLOC)      //новый букинг полностью внутри существующего
                    );
        }

        private static bool IsBookingActive(Booking booking, DateTime currentUtcTime)
        {
            var company = booking.Product.Company ?? booking.Worker.CompanyWorkAssignments.FirstOrDefault()?.Company; ;
            if (company == null)
            {
                return false;
            }

            var currentTimeInCompanyTz = currentUtcTime.Add(company.TimeZoneFromUTCOffset);
            return booking.EndDateLOC > currentTimeInCompanyTz;
        }
    }
}
