using BookingService.DB;
using BookingService.Entities;
using MassTransit;
using Shared.Messages.Schedule;

namespace BookingService.Consumers
{
    public class CreateScheduleIntervalConsumer : IConsumer<ScheduleIntervalCreated>
    {
        private readonly Context dbcontext;
        public CreateScheduleIntervalConsumer(Context context)
        {
            dbcontext = context;
        }
        public async Task Consume(ConsumeContext<ScheduleIntervalCreated> context)
        {
            var message = context.Message;
            await dbcontext.ScheduleIntervals.AddAsync(new ScheduleInterval { Id = message.IntervalId, IntervalDuration = message.Duration, StartTimeLOC = message.StartTimeLOC, WeekDay = message.WeekDay });
            await dbcontext.SaveChangesAsync();
        }
    }
}
