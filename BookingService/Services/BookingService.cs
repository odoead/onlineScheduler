using BookingService.DB;
using BookingService.DTO;
using BookingService.Entities;
using BookingService.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.Events.Booking;
using Shared.Exceptions.custom_exceptions;

namespace BookingService.Services
{
    public class BookingService : IBookingService
    {
        private readonly Context dbcontext;
        private readonly IPublishEndpoint publishEndpoint;

        public BookingService(Context context, IPublishEndpoint endpoint)
        {
            dbcontext = context;
            publishEndpoint = endpoint;
        }

        private async Task<bool> IsOverlappingBookingsAsync(TimeSpan newStartTime, TimeSpan newIntervalDuration, int weekDay, string employeeId, int? excludedBookingId = null)
        {
            var newIntervalEnd = newStartTime + newIntervalDuration;
            return await dbcontext.Bookings.Where(i => i.WorkerId == employeeId && i.StartDateLOC.DayOfWeek == (DayOfWeek)weekDay &&
            (!excludedBookingId.HasValue || i.Id != excludedBookingId) && i.Status != BookingStatus.Canceled)
                .AnyAsync(i => (newStartTime >= i.StartDateLOC.TimeOfDay && newStartTime < i.EndDateLOC.TimeOfDay) ||
                (newIntervalEnd > i.StartDateLOC.TimeOfDay && newIntervalEnd <= i.EndDateLOC.TimeOfDay) ||
                (newStartTime <= i.StartDateLOC.TimeOfDay && newIntervalEnd >= i.EndDateLOC.TimeOfDay)
                );
        }


        public async Task AddBookingAsync(AddBookingDTO addBooking)
        {

            var productDuration = await dbcontext.Products.Where(q => q.Id == addBooking.ProductId).Select(q => q.Duration).FirstAsync();
            var startTime = addBooking.BookingTimeLOC.TimeOfDay;
            var weekDay = (int)addBooking.BookingTimeLOC.DayOfWeek;
            var booking = new Booking
            {
                ClientId = addBooking.ClientId,
                WorkerId = addBooking.WorkerId,
                CompanyId = addBooking.CompanyId,
                ProductId = addBooking.ProductId,
                Status = BookingStatus.Created,

                StartDateLOC = addBooking.BookingTimeLOC,
                EndDateLOC = addBooking.BookingTimeLOC.Add(productDuration),
            };

            if (await IsOverlappingBookingsAsync(startTime, productDuration, weekDay, addBooking.WorkerId))
                throw new InvalidOperationException("The booking overlaps with an existing booking.");

            dbcontext.Bookings.Add(booking);
            await dbcontext.SaveChangesAsync();

            await publishEndpoint.Publish(new BookingCreated
            {

            });
        }

        public async Task EditBookingAsync(EditBookingDTO editBookingDTO)
        {
            var booking = await dbcontext.Bookings.FindAsync(editBookingDTO.Id) ?? throw new NotFoundException("Invalid booking ID " + editBookingDTO.Id);
            var product = await dbcontext.Products.FindAsync(editBookingDTO.ProductId) ?? throw new NotFoundException("Invalid product ID " + editBookingDTO.ProductId);

            // Calculate the new start and end times
            var startTime = editBookingDTO.BookingTimeLOC.TimeOfDay;
            var endTime = startTime + product.Duration;
            var weekDay = (int)editBookingDTO.BookingTimeLOC.DayOfWeek;

            if (await IsOverlappingBookingsAsync(startTime, product.Duration, weekDay, editBookingDTO.WorkerId, editBookingDTO.Id))
            {
                throw new Exception("The booking overlaps with an existing booking.");
            }

            // Update booking details
            booking.ProductId = editBookingDTO.ProductId;
            booking.WorkerId = editBookingDTO.WorkerId;
            booking.StartDateLOC = editBookingDTO.BookingTimeLOC;
            booking.EndDateLOC = editBookingDTO.BookingTimeLOC.Add(product.Duration);

            await dbcontext.SaveChangesAsync();
            await publishEndpoint.Publish(new BookingEdited
            {

            });
        }

        public async Task ChangeBookingStatusASync(int bookingId, BookingStatus newStatus)
        {
            var booking = await dbcontext.Bookings.FindAsync(bookingId) ?? throw new NotFoundException("Invalid booking ID" + bookingId);

            booking.Status = newStatus;
            await dbcontext.SaveChangesAsync();
            await publishEndpoint.Publish(new BookingStatusChanged
            {

            });
        }
    }
}
