using BookingService.DB;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.Events.Booking;

namespace BookingService.Consumers
{
    public class BookingStatisticsRequestConsumer : IConsumer<BookingStatisticsRequest>
    {
        private readonly Context dbcontext;

        public BookingStatisticsRequestConsumer(Context context)
        {
            dbcontext = context;
        }

        public async Task Consume(ConsumeContext<BookingStatisticsRequest> context)
        {
            var request = context.Message;
            var bookings = await dbcontext.Bookings.Where(b => request.WorkerIds.Contains(b.WorkerId) && b.StartDateUTC >= request.StartDate && b.StartDateUTC <= request.EndDate)
                .ToListAsync();

            var statusCounts = Enum.GetValues<BookingStatus>()
                .ToDictionary(status => status, status => bookings.Count(b => b.Status == status));

            await context.RespondAsync(new BookingStatisticsRequestResult
            {
                TotalBookings = bookings.Count,
                CompletedBookings = statusCounts.TryGetValue(BookingStatus.CONFIRMED, out var confirmed) ? confirmed : 0,
                CancelledBookings = statusCounts.TryGetValue(BookingStatus.CANCELED, out var canceled) ? canceled : 0,
                PendingBookings = statusCounts.TryGetValue(BookingStatus.CREATED, out var created) ? created : 0,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
            });
        }
    }
}
