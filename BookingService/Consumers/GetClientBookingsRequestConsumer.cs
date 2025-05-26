using BookingService.DB;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events.Booking;
using Shared.Events.User;
using Shared.Exceptions.custom_exceptions;

namespace BookingService.Consumers
{
    public class GetClientBookingsRequestConsumer : IConsumer<GetClientBookingsRequested>
    {
        private readonly Context dbcontext;
        private readonly IRequestClient<UserIdRequested> userClient;

        public GetClientBookingsRequestConsumer(Context context, IRequestClient<UserIdRequested> client)
        {
            dbcontext = context;
            userClient = client;
        }
        public async Task Consume(ConsumeContext<GetClientBookingsRequested> context)
        {
            var message = context.Message;
            var bookings = await dbcontext.Bookings.Where(q => q.ClientId == message.clientId && q.Service == Entities.ServiceType.SCHEDULE)
                .OrderBy(q => q.StartDateUTC).ToListAsync();

            var clientBookings = new List<ClientBooking_ScheduleService>();

            foreach (var booking in bookings)
            {
                var userData = await GetUserData(booking.ClientId);
                var clientBooking = new ClientBooking_ScheduleService
                {
                    EndDateUTC = booking.EndDateUTC.Value,
                    ProductId = booking.ProductId,
                    Status = booking.Status.ToString(),
                    StartDateUTC = booking.StartDateUTC,
                    BookingId = booking.Id,
                };
                clientBookings.Add(clientBooking);
            }

            await context.RespondAsync<GetClientBookingsRequestResult>(new GetClientBookingsRequestResult { Bookings = clientBookings });
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
