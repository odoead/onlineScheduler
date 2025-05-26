using BookingService.DB;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events.Booking;
using Shared.Events.User;
using Shared.Exceptions.custom_exceptions;

namespace BookingService.Consumers
{
    public class WorkerBookingsRequestConsumer : IConsumer<WorkerBookingsRequested>
    {
        private readonly Context dbcontext;
        private readonly IRequestClient<UserIdRequested> userClient;

        public WorkerBookingsRequestConsumer(Context context, IRequestClient<UserIdRequested> client)
        {
            dbcontext = context;
            userClient = client;
        }
        public async Task Consume(ConsumeContext<WorkerBookingsRequested> context)
        {
            var message = context.Message;

            var bookings = await dbcontext.Bookings.Where(q => q.WorkerId == message.workerId && q.Service == Entities.ServiceType.SCHEDULE)
                .OrderBy(q => q.StartDateUTC).ToListAsync();

            var workerBookings = new List<WorkerBooking_ScheduleService>();

            foreach (var booking in bookings)
            {
                var userData = await GetUserData(booking.ClientId);
                var workerBooking = new WorkerBooking_ScheduleService
                {
                    EndDateLOC = booking.EndDateUTC.Value,
                    ProductId = booking.ProductId,
                    Status = booking.Status.ToString(),
                    StartDateLOC = booking.StartDateUTC,
                    CustomerEmail = userData.Email,
                    CustomerName = userData.UserName,
                    BookingId = booking.Id,
                };
                workerBookings.Add(workerBooking);
            }

            await context.RespondAsync<WorkerBookingsRequestResult>(new WorkerBookingsRequestResult { Bookings = workerBookings });
        }
        private async Task<UserIdRequestResult> GetUserData(string id)
        {
            var response = await userClient.GetResponse<UserIdRequestResult, UserIdRequestedNotFoundResult>(new UserIdRequested { Id = id });
            switch (response)
            {
                case var r when r.Message is UserIdRequestResult result:
                    return result;

                case var r when r.Message is UserIdRequestedNotFoundResult notFoundResult:
                    throw new BadRequestException("User with id " + id + " not found");

                default:
                    throw new InvalidOperationException("Unknown response type received.");
            }

        }
    }
}
