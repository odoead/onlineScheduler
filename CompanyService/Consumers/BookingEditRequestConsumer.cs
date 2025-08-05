using CompanyService.DB;
using CompanyService.Helpers;
using CompanyService.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events.Booking;
using Shared.Exceptions.custom_exceptions;

namespace CompanyService.Consumers
{
    public class BookingEditRequestConsumer : IConsumer<BookingEditRequest>
    {
        private readonly Context dbcontext;
        private readonly IBookingValidationService validationService;
        public BookingEditRequestConsumer(Context context, IBookingValidationService validationService)
        {
            dbcontext = context;
            this.validationService = validationService;
        }
        public async Task Consume(ConsumeContext<BookingEditRequest> context)
        {
            var message = context.Message;

            var booking = await dbcontext.Bookings.Include(b => b.Worker).FirstOrDefaultAsync(b => b.Id == message.BookingId) ??
                throw new NotFoundException("Booking not found"); ;

            var newEndDateLOC = message.StartDateLOC.Add(booking.Product.Duration);

            if (message.WorkerId == booking.WorkerId)
            {
                // Same worker overlap check
                if (await validationService.HasOverlappingBookings(
                    booking.WorkerId,
                    message.StartDateLOC,
                    newEndDateLOC,
                    excludeBookingId: booking.Id))
                {
                    throw new BadRequestException("New booking time overlaps with existing bookings");
                }
            }
            else
            {
                // New worker overlap check
                if (await validationService.HasOverlappingBookings(
                    message.WorkerId,
                    message.StartDateLOC,
                    newEndDateLOC))
                {
                    throw new BadRequestException("New worker's booking time overlaps with existing bookings");
                }
            }

            if (!await validationService.IsBookingTimeAvailableAsync(
                    message.StartDateLOC,
                    newEndDateLOC,
                    booking.Product.CompanyId,
                    message.WorkerId))
            {
                throw new BadRequestException("Invalid booking time for the worker or company");
            }

            booking.StartDateLOC = message.StartDateLOC;
            booking.EndDateLOC = newEndDateLOC;
            booking.WorkerId = message.WorkerId;
            booking.ProductId = message.ProductId;



            // Get the company's location for timezone conversion
            var company = await dbcontext.Companies.Include(c => c.Location).FirstOrDefaultAsync(c => c.Id == booking.Product.CompanyId)
                ?? throw new NotFoundException("Company not found");

            double latitude = company.Location.Coordinates.Y;
            double longitude = company.Location.Coordinates.X;

            var startTimeUTC = GetUTCTimeByLocation(message.StartDateLOC, latitude, longitude);

            dbcontext.Bookings.Update(booking);
            await dbcontext.SaveChangesAsync();

            await context.RespondAsync<BookingEditRequestResult>(new BookingEditRequestResult { IsEdited = true, StartDateUTC = startTimeUTC });
        }
        private DateTime GetUTCTimeByLocation(DateTime startDateLOC, double latitude, double longitude)
        {
            var tzInfo = TimezoneConverter.GetTimezoneFromLocation(longitude, latitude);
            return TimeZoneInfo.ConvertTimeToUtc(startDateLOC, tzInfo);
        }
    }
}
