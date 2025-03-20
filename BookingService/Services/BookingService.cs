using BookingService.DB;
using BookingService.Entities;
using BookingService.Interfaces;
using MassTransit;
using Shared.Data;
using Shared.Events.Booking;
using Shared.Events.User;
using Shared.Exceptions.custom_exceptions;

namespace BookingService.Services
{
    public class BookingService : IBookingService
    {
        private readonly Context dbcontext;
        private readonly IPublishEndpoint publishEndpoint;
        IRequestClient<IsValidBookingTimeRequested> IsValidBookingTimeClient;// check if booking can be created
        IRequestClient<BookingConfirmationRequested> BookingConfirmationclient;// create booking and produce success-fail
        IRequestClient<UserEmailRequested> UserEmailclient;
        IRequestClient<BookingEditRequest> BookingEditClient;
        IRequestClient<RabbitTestRequest> RabbitTestClient;

        public BookingService(Context context, IPublishEndpoint endpoint, IRequestClient<IsValidBookingTimeRequested> client,
            IRequestClient<BookingConfirmationRequested> client2, IRequestClient<UserEmailRequested> userEmailclient,
            IRequestClient<BookingEditRequest> bookingEditClient,
            IRequestClient<RabbitTestRequest> test)
        {
            dbcontext = context;
            publishEndpoint = endpoint;
            IsValidBookingTimeClient = client;
            BookingConfirmationclient = client2;
            UserEmailclient = userEmailclient;
            BookingEditClient = bookingEditClient;
            RabbitTestClient = test;
        }

        public async Task AddBookingAsync(DateTime BookingTimeLOC, string WorkerId, string ClientEmail, int ProductId, TimeSpan? Duration = null)
        {
            var response = await UserEmailclient.GetResponse<UserEmailRequestResult, UserEmailRequestedNotFoundResult>(new UserEmailRequested { Email = ClientEmail });
            string clientId;
            switch (response)
            {
                case var r when r.Message is UserEmailRequestResult result:
                    clientId = result.Id;
                    break;
                case var r when r.Message is UserEmailRequestedNotFoundResult notFoundResult:
                    throw new BadRequestException("User with email " + ClientEmail + " not found");

                default:
                    throw new InvalidOperationException("Unknown response type received.");
            }
            var booking = new Booking
            {
                ClientId = clientId,
                WorkerId = WorkerId,
                ProductId = ProductId,
                Status = BookingStatus.CREATED,

                StartDateLOC = BookingTimeLOC,

            };
            if (Duration != null)
            {
                var responseIs = await IsValidBookingTimeClient.GetResponse<IsValidBookingTimeRequestResult>(new IsValidBookingTimeRequested
                {
                    StartDateLOC = BookingTimeLOC,
                    EndDateLOC = BookingTimeLOC.Add(Duration.Value),
                    ProductId = ProductId,
                    WorkerId = WorkerId
                });
                if (responseIs.Message.IsValid == false)
                {
                    throw new BadRequestException("The booking overlaps with an existing booking.");
                }
                booking.EndDateLOC = BookingTimeLOC.Add(Duration.Value);
            }

            dbcontext.Bookings.Add(booking);
            await dbcontext.SaveChangesAsync();

            await publishEndpoint.Publish(new BookingCreated
            {
                WorkerId = WorkerId,
                BookingId = booking.Id,
                ClientId = clientId,
                EndDateLOC = booking.EndDateLOC,
                StartDateLOC = booking.StartDateLOC,
                ProductId = ProductId,
            });
        }

        public async Task EditBookingAsync(int Id, DateTime BookingTimeLOC, string WorkerId)
        {
            var booking = await dbcontext.Bookings.FindAsync(Id) ?? throw new BadRequestException("Invalid booking ID " + Id);

            if (booking.Status != BookingStatus.CREATED)
            {
                throw new BadRequestException("Only Created bookings can be edited.");
            }

            if (booking.EndDateLOC != null)
            {
                var duration = booking.EndDateLOC - booking.StartDateLOC;//calculate duration based on start-end diff
                var response = await BookingEditClient.GetResponse<BookingEditRequestResult>(new BookingEditRequest
                {
                    StartDateLOC = BookingTimeLOC,
                    EndDateLOC = BookingTimeLOC.Add(duration.Value),
                    WorkerId = WorkerId,
                    ProductId = booking.ProductId,
                    BookingId = booking.Id,
                });
                if (response.Message.IsEdited == false)
                {
                    throw new BadRequestException("Booking edit error. Id: " + booking.Id);
                }
                booking.EndDateLOC = BookingTimeLOC.Add(duration.Value);

            }

            booking.WorkerId = WorkerId;
            booking.StartDateLOC = BookingTimeLOC;

            await dbcontext.SaveChangesAsync();

        }

        public async Task ChangeBookingStatusAsync(int bookingId, int newStatus)
        {
            var booking = await dbcontext.Bookings.FindAsync(bookingId) ?? throw new NotFoundException("Invalid booking ID" + bookingId);
            if (!Enum.IsDefined(typeof(BookingStatus), newStatus))
            {
                throw new BadRequestException("This status doesnt exist ");
            }

            if ((BookingStatus)booking.Status != BookingStatus.CREATED)
            {
                throw new BadRequestException("You can only change the booking status of unconfirmed orders ");
            }

            if (booking.EndDateLOC != null)
            {
                switch ((BookingStatus)newStatus)
                {
                    case BookingStatus.CANCELED:

                        booking.Status = BookingStatus.CANCELED;
                        await publishEndpoint.Publish(new BookingCanceled
                        {
                            BookingId = booking.Id,
                            WorkerId = booking.WorkerId,
                            ProductId = booking.ProductId,
                            OriginalStatus = booking.Status,
                            EndDateLOC = booking.EndDateLOC,
                            StartDateLOC = booking.StartDateLOC
                        });
                        break;

                    case BookingStatus.CONFIRMED:
                        booking.Status = BookingStatus.CONFIRMED;
                        var confirmationResponse = await BookingConfirmationclient.GetResponse<BookingConfirmationRequestResult>(new BookingConfirmationRequested
                        {
                            BookingId = booking.Id,
                            WorkerId = booking.WorkerId,
                            ProductId = booking.ProductId,
                            StartDateLOC = booking.StartDateLOC,
                            EndDateLOC = booking.EndDateLOC.Value,
                        });

                        if (!confirmationResponse.Message.IsRegistered)
                        {
                            throw new BadRequestException("Booking confirmation failed");
                        }
                        await dbcontext.SaveChangesAsync();
                        await publishEndpoint.Publish(new BookingConfirmed
                        {
                            BookingId = booking.Id,
                            WorkerId = booking.WorkerId,
                            ProductId = booking.ProductId,
                            StartDateLOC = booking.StartDateLOC,
                            EndDateLOC = booking.EndDateLOC
                        });
                        break;
                }
            }
        }

        public async Task<string> GetRabbitDataTest()
        {
            var response = await RabbitTestClient.GetResponse<RabbitTestRequestResult>(new RabbitTestRequest
            {
                 val="hi"
            });
            return response.Message.returnVal;
        }
    }
}
