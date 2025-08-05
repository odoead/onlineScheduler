using BookingService.DB;
using BookingService.Entities;
using BookingService.Interfaces;
using MassTransit;
using Shared.Data;
using Shared.Events.Booking;
using Shared.Events.Company;
using Shared.Events.User;
using Shared.Exceptions.custom_exceptions;

namespace BookingService.Services
{
    public class BookingService : IBookingService
    {
        private readonly Context dbcontext;
        private readonly IPublishEndpoint publishEndpoint;
        private readonly IRequestClient<IsValidBookingTimeRequested> IsValidBookingTimeClient;// To check if booking can be created
        private readonly IRequestClient<BookingConfirmationRequested> BookingConfirmationclient;// To create booking and produce success-fail
        private readonly IRequestClient<UserEmailRequested> UserEmailclient;
        private readonly IRequestClient<BookingEditRequest> BookingEditClient;
        private readonly IRequestClient<RabbitTestRequest> RabbitTestClient;
        private readonly IRequestClient<GetCompanyTimeZoneRequest> companyTimeZoneClient;

        public BookingService(Context context, IPublishEndpoint endpoint, IRequestClient<IsValidBookingTimeRequested> client,
            IRequestClient<BookingConfirmationRequested> client2, IRequestClient<UserEmailRequested> userEmailclient,
            IRequestClient<BookingEditRequest> bookingEditClient,
            IRequestClient<RabbitTestRequest> test, IRequestClient<GetCompanyTimeZoneRequest> timezoneClient)
        {
            dbcontext = context;
            publishEndpoint = endpoint;
            IsValidBookingTimeClient = client;
            BookingConfirmationclient = client2;
            UserEmailclient = userEmailclient;
            BookingEditClient = bookingEditClient;
            RabbitTestClient = test;
            companyTimeZoneClient = timezoneClient;
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

            var tzInfo = await companyTimeZoneClient.GetResponse<GetCompanyTimeZoneResult>(new GetCompanyTimeZoneRequest { ProductId = ProductId });
            var StartDateUTC = TimeZoneInfo.ConvertTimeToUtc(BookingTimeLOC, tzInfo.Message.TimeZone);

            var booking = new Booking
            {
                ClientId = clientId,
                WorkerId = WorkerId,
                ProductId = ProductId,
                Status = BookingStatus.CREATED,
                EndDateUTC = StartDateUTC + Duration,
                StartDateUTC = StartDateUTC,
                Service = ServiceType.SCHEDULE,

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
                booking.EndDateUTC = StartDateUTC.Add(Duration.Value);
            }

            dbcontext.Bookings.Add(booking);
            await dbcontext.SaveChangesAsync();

            await publishEndpoint.Publish(new BookingCreated
            {
                BookingsWorkerId = WorkerId,
                BookingId = booking.Id,
                BookingsClientId = clientId,
                BookingEndDateLOC = BookingTimeLOC + Duration,
                BookingStartDateLOC = BookingTimeLOC,
                BookingProductId = ProductId,
                BookingStartDateUTC = StartDateUTC,
                BookingEndDateUTC = booking.EndDateUTC,
            });
        }

        public async Task EditBookingAsync(int Id, DateTime BookingTimeLOC, string WorkerId)
        {
            var booking = await dbcontext.Bookings.FindAsync(Id) ?? throw new BadRequestException("Invalid booking ID " + Id);

            if (booking.Status != BookingStatus.CREATED)
            {
                throw new BadRequestException("Only Created bookings can be edited.");
            }

            var tzInfo = await companyTimeZoneClient.GetResponse<GetCompanyTimeZoneResult>(new GetCompanyTimeZoneRequest { ProductId = booking.ProductId });
            var StartDateUTC = TimeZoneInfo.ConvertTimeToUtc(BookingTimeLOC, tzInfo.Message.TimeZone);

            var duration = booking.EndDateUTC - booking.StartDateUTC;// calculate duration based on start-end diff
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
            booking.EndDateUTC = StartDateUTC.Add(duration.Value);
            booking.WorkerId = WorkerId;
            booking.StartDateUTC = StartDateUTC;

            await dbcontext.SaveChangesAsync();
            await publishEndpoint.Publish<BookingEditCreatedRequest>(new BookingEditCreatedRequest
            {

                BookingId = booking.Id,
                WorkerId = booking.WorkerId,
                ProductId = booking.ProductId,
                StartDateLOC = BookingTimeLOC,
                EndDateLOC = BookingTimeLOC + duration.Value,
                StartDateUTC = StartDateUTC,
                EndDateUTC = StartDateUTC + duration.Value,

            });
        }

        public async Task ChangeBookingStatusAsync(int bookingId, int newStatus)
        {
            var booking = await dbcontext.Bookings.FindAsync(bookingId) ?? throw new NotFoundException("Invalid booking ID" + bookingId);
            if (!Enum.IsDefined(typeof(BookingStatus), newStatus))
            {
                throw new BadRequestException("This status doesnt exist ");
            }

            if ((BookingStatus)booking.Status == BookingStatus.CONFIRMED)
            {
                throw new BadRequestException("You can only change the booking status of unconfirmed orders ");
            }

            var tzInfo = await companyTimeZoneClient.GetResponse<GetCompanyTimeZoneResult>(new GetCompanyTimeZoneRequest { ProductId = booking.ProductId });
            var StartDateLOC = TimeZoneInfo.ConvertTimeFromUtc(booking.StartDateUTC, tzInfo.Message.TimeZone);
            DateTime EndDateLOC = TimeZoneInfo.ConvertTimeFromUtc((DateTime)booking.EndDateUTC, tzInfo.Message.TimeZone);

            if (booking.EndDateUTC != null)
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
                            EndDateLOC = EndDateLOC,
                            StartDateLOC = StartDateLOC,
                            StartDateUTC = booking.StartDateUTC,
                            EndDateUTC = booking.EndDateUTC,
                        });
                        break;

                    case BookingStatus.CONFIRMED:
                        booking.Status = BookingStatus.CONFIRMED;
                        var confirmationResponse = await BookingConfirmationclient.GetResponse<BookingConfirmationRequestResult>(new BookingConfirmationRequested
                        {
                            BookingId = booking.Id,
                            WorkerId = booking.WorkerId,
                            ProductId = booking.ProductId,
                            StartDateLOC = StartDateLOC,
                            EndDateLOC = EndDateLOC,
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
                            BookingStartDateLOC = StartDateLOC,
                            BookingEndDateLOC = EndDateLOC,
                            BookingStartDateUTC = booking.StartDateUTC,
                        });
                        break;
                }
            }
        }

        public async Task<string> GetRabbitDataTest()
        {
            var response = await RabbitTestClient.GetResponse<RabbitTestRequestResult>(new RabbitTestRequest
            {
                val = "hi"
            });
            return response.Message.returnVal;
        }

        public async Task CancelBookingAsync(int bookingId)
        {
            var booking = await dbcontext.Bookings.FindAsync(bookingId)
                          ?? throw new NotFoundException("Invalid booking ID " + bookingId);

            //allow cancellation for bookings with created status 
            if (booking.Status != BookingStatus.CREATED)
            {
                throw new BadRequestException("Only bookings with status CREATED can be canceled. Booking ID: " + bookingId);
            }

            var tzInfo = await companyTimeZoneClient.GetResponse<GetCompanyTimeZoneResult>(new GetCompanyTimeZoneRequest { ProductId = booking.ProductId });
            var StartDateLOC = TimeZoneInfo.ConvertTimeFromUtc(booking.StartDateUTC, tzInfo.Message.TimeZone);
            DateTime EndDateLOC = TimeZoneInfo.ConvertTimeFromUtc((DateTime)booking.EndDateUTC, tzInfo.Message.TimeZone);

            booking.Status = BookingStatus.CANCELED;

            await dbcontext.SaveChangesAsync();

            await publishEndpoint.Publish(new BookingCanceled
            {


                BookingId = booking.Id,
                StartDateUTC = booking.StartDateUTC,
                WorkerId = booking.WorkerId,
                ProductId = booking.ProductId,
                OriginalStatus = BookingStatus.CREATED,
                StartDateLOC = StartDateLOC,
                EndDateLOC = EndDateLOC,
                EndDateUTC = booking.EndDateUTC,


            });
        }
    }
}
