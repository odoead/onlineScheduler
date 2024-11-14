using MassTransit;
using ScheduleService.DB;

namespace ScheduleService.Consumers
{
    public class UpdateBookingConsumer : IConsumer<UpdateBookingConsumer>
    {//change HasActiveBooking
        private readonly Context dbcontext;

        public Task Consume(ConsumeContext<UpdateBookingConsumer> context)
        {
            throw new NotImplementedException();
        }
    }
}
