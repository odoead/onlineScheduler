using MassTransit;
using ScheduleService.DB;
using ScheduleService.Entities;
using Shared.Messages.Company;

namespace ScheduleService.Consumers
{
    public class CreateCompanyConsumer : IConsumer<CompanyCreated>
    {//add new schedule intervals for users of company 
        private readonly Context dbcontext;
        public CreateCompanyConsumer(Context context)
        {
            dbcontext = context;
        }
        public async Task Consume(ConsumeContext<CompanyCreated> context)
        {
            var message = context.Message;

            foreach (var workerId in message.EmployeeIds)
            {
                foreach (var day in message.WorkingDays)
                {
                    var scheduleInterval = new ScheduleInterval
                    {
                        WeekDay = ((int)day),
                        StartTimeLOC = message.OpeningTimeLOC,
                        IntervalDuration = message.ClosingTimeLOC - message.OpeningTimeLOC,
                        IntervalType = ((int)IntervalType.Work),
                        EmployeeId = workerId,
                        Id = message.CompanyId,
                    };

                    dbcontext.ScheduleIntervals.Add(scheduleInterval);
                }
            }
            await dbcontext.SaveChangesAsync();
           // await context.RespondAsync(message);
        }
    }
}
