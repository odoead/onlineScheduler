using MassTransit;
using Microsoft.EntityFrameworkCore;
using ScheduleService.DB;
using Shared.Events.Booking;

namespace ScheduleService.Consumers
{
    public class CreateBookingConsumer : IConsumer<BookingCreated>
    {
        private readonly Context dbcontext;
        public CreateBookingConsumer(Context context)
        {
            dbcontext = context;
        }
        public async Task Consume(ConsumeContext<BookingCreated> context)
        {
            var mess = context.Message;
            var existingBooking = await dbcontext.Bookings.FirstOrDefaultAsync(b => b.Id == mess.BookingId);
            if (existingBooking != null)
            {
                return;
            }
            existingBooking.StartDateLOC = mess.StartDateLOC;
            existingBooking.EndDateLOC = mess.EndDateLOC;


            var productTitle = GetProductTitleById(mess.ProductId); 
            existingBooking.ProductTitle = productTitle;

            var employee = dbcontext.Employees
                .FirstOrDefault(e => e.Id == mess.WorkerId);
            if (employee != null)
            {
                existingBooking.Employee = employee;
                existingBooking.EmployeeId = employee.Id;
            }


            await dbcontext.SaveChangesAsync();


        }
    }
}
