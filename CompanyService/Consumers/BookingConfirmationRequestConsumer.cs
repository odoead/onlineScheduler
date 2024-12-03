using CompanyService.DB;
using CompanyService.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events.Booking;

namespace CompanyService.Consumers
{
    /// <summary>
    /// Add booking to db when booking is confirmed
    /// </summary>
    public class BookingConfirmationRequestConsumer : IConsumer<BookingConfirmationRequested>
    {
        private readonly Context dbcontext;
        private readonly IBookingValidationService validationService;
        public BookingConfirmationRequestConsumer(Context context, IBookingValidationService bookingValidationService)
        {
            dbcontext = context;
            validationService = bookingValidationService;
        }
        public async Task Consume(ConsumeContext<BookingConfirmationRequested> context)
        {
            var message = context.Message;
            var companyId = await dbcontext.Companies.Where(company => company.Products.Any(p => p.Id == message.ProductId))
                .Select(q => q.Id).FirstOrDefaultAsync();
            var result = await validationService.IsValidBookingTime(message.StartDateLOC, message.EndDateLOC, companyId, message.WorkerId);

            if (result)
            {
                var booking = new Entities.Booking
                {
                    BookingServiceId = message.BookingId,
                    EndDateLOC = message.EndDateLOC,
                    ProductId = message.ProductId,
                    StartDateLOC = message.StartDateLOC,
                    WorkerId = message.WorkerId,
                    ScheduleInterval = await dbcontext.ScheduleIntervals.Where(q => q.WorkerId == message.WorkerId && q.CompanyId == companyId &&
                    q.StartTimeLOC <= message.StartDateLOC.TimeOfDay && q.StartTimeLOC < message.EndDateLOC.TimeOfDay &&
                    q.FinishTimeLOC > message.StartDateLOC.TimeOfDay && q.FinishTimeLOC <= message.EndDateLOC.TimeOfDay).FirstOrDefaultAsync()
                };
                dbcontext.Bookings.Add(booking);
                await dbcontext.SaveChangesAsync();

            }

            await context.RespondAsync<BookingConfirmationRequestResult>(new BookingConfirmationRequestResult { IsRegistered = result, });
        }

    }
}
