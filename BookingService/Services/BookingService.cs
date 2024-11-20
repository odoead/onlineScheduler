using BookingService.DB;
using BookingService.DTO;
using BookingService.Entities;
using BookingService.Interfaces;
using MassTransit;
using Shared.Data;
using Shared.Events.Booking;
using Shared.Exceptions.custom_exceptions;

namespace BookingService.Services
{
    public class BookingService : IBookingService
    {
        private readonly Context dbcontext;
        private readonly IPublishEndpoint publishEndpoint;
        IRequestClient<IsValidBookingTimeRequested> _client;// check if booking can be created
        IRequestClient<BookingConfirmationRequested> _client2;// create booking and produce success-fail

        public BookingService(Context context, IPublishEndpoint endpoint, IRequestClient<IsValidBookingTimeRequested> client, IRequestClient<BookingConfirmationRequested> client2)
        {
            dbcontext = context;
            publishEndpoint = endpoint;
            _client = client;
            _client2 = client2;
        }




        public async Task AddBookingAsync(AddBookingDTO addBooking)
        {
            var booking = new Booking
            {
                ClientId = addBooking.ClientId,
                WorkerId = addBooking.WorkerId,
                ProductId = addBooking.ProductId,
                Status = BookingStatus.Created,

                StartDateLOC = addBooking.BookingTimeLOC,

            };
            if (addBooking.Duration != null)
            {
                var response = await _client.GetResponse<IsValidBookingTimeRequestResult>(new IsValidBookingTimeRequested
                {
                    StartDateLOC = addBooking.BookingTimeLOC,
                    EndDateLOC = addBooking.BookingTimeLOC.Add(addBooking.Duration.Value),
                    ProductId = addBooking.ProductId,
                    WorkerId = addBooking.WorkerId
                });
                if (response.Message.IsValid == false)
                {
                    throw new BadRequestException("The booking overlaps with an existing booking.");
                }
                booking.EndDateLOC = addBooking.BookingTimeLOC.Add(addBooking.Duration.Value);
            }

            dbcontext.Bookings.Add(booking);
            await dbcontext.SaveChangesAsync();

            await publishEndpoint.Publish(new BookingCreated
            {
                WorkerId = addBooking.WorkerId,
                BookingId = booking.Id,
                ClientId = addBooking.ClientId,
                EndDateLOC = booking.EndDateLOC,
                StartDateLOC = booking.StartDateLOC,
                ProductId = addBooking.ProductId,
            });
        }

        public async Task EditBookingAsync(EditBookingDTO editBookingDTO)
        {
            var booking = await dbcontext.Bookings.FindAsync(editBookingDTO.Id) ?? throw new BadRequestException("Invalid booking ID " + editBookingDTO.Id);

            if (booking.Status != BookingStatus.Created)
            {
                throw new BadRequestException("Only Created bookings can be edited.");
            }

            if (booking.EndDateLOC != null)
            {
                var duration = booking.EndDateLOC - booking.StartDateLOC;//calculate duration based on start-end diff
                var response = await _client.GetResponse<IsValidBookingTimeRequestResult>(new IsValidBookingTimeRequested
                {
                    StartDateLOC = editBookingDTO.BookingTimeLOC,
                    EndDateLOC = editBookingDTO.BookingTimeLOC.Add(duration.Value),
                    WorkerId = editBookingDTO.WorkerId,
                    ProductId = booking.ProductId
                });
                if (response.Message.IsValid == false)
                {
                    throw new BadRequestException("The booking overlaps with an existing booking.");
                }
                booking.EndDateLOC = editBookingDTO.BookingTimeLOC.Add(duration.Value);

            }

            booking.WorkerId = editBookingDTO.WorkerId;
            booking.StartDateLOC = editBookingDTO.BookingTimeLOC;

            await dbcontext.SaveChangesAsync();
            await publishEndpoint.Publish(new BookingEdited
            {
                BookingId = booking.Id,
                WorkerId = booking.WorkerId,
                StartDateLOC = booking.StartDateLOC,
                EndDateLOC = booking.EndDateLOC.Value,
                ProductId = booking.ProductId
            });
        }

        public async Task ChangeBookingStatusASync(int bookingId, int newStatus)
        {
            var booking = await dbcontext.Bookings.FindAsync(bookingId) ?? throw new NotFoundException("Invalid booking ID" + bookingId);
            if (!Enum.IsDefined(typeof(BookingStatus), newStatus))
            {
                throw new BadRequestException("This status doesnt exist ");
            }

            if (booking.EndDateLOC != null)
            {
                switch ((BookingStatus)newStatus)
                {
                    case BookingStatus.Canceled:

                        booking.Status = BookingStatus.Canceled;
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

                    case BookingStatus.Confirmed:
                        booking.Status = BookingStatus.Confirmed;
                        var confirmationResponse = await _client2.GetResponse<BookingConfirmationRequestResult>(new BookingConfirmationRequested
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
                        booking.Status = BookingStatus.Created;
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
    }
}
