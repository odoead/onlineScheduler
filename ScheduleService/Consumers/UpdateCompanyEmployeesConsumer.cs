using MassTransit;
using Microsoft.EntityFrameworkCore;
using ScheduleService.DB;
using ScheduleService.Entities;
using Shared.Messages.Company;

namespace ScheduleService.Consumers
{
    public class UpdateCompanyEmployeesConsumer : IConsumer<UpdatedCompanyEmployees>
    {//add/remove schedule intervals for new employees

        private readonly Context dbcontext;

        public UpdateCompanyEmployeesConsumer(Context context)
        {
            dbcontext = context;
        }

        public async Task Consume(ConsumeContext<UpdatedCompanyEmployees> context)
        {
            var message = context.Message;
            var currentIntervals = await dbcontext.ScheduleIntervals
                .Where(i => message.EmployeeIds.Contains(i.EmployeeId))
                .ToListAsync();

            var currentEmployeeIds = currentIntervals.Select(i => i.EmployeeId).Distinct().ToList();

            var employeeIdsToRemove = currentEmployeeIds.Except(message.EmployeeIds).ToList();
            var employeeIdsToAdd = message.EmployeeIds.Except(currentEmployeeIds).ToList();

            // Remove intervals for employees no longer part of the company
            var intervalsToRemove = currentIntervals
                .Where(i => employeeIdsToRemove.Contains(i.EmployeeId))
                .ToList();
            dbcontext.ScheduleIntervals.RemoveRange(intervalsToRemove);

            // Add intervals for newly added employees
            foreach (var newEmployeeId in employeeIdsToAdd)
            {
                foreach (var day in message.WorkingDays)
                {
                    var scheduleInterval = new ScheduleInterval
                    {
                        WeekDay = ((int)day),
                        StartTimeLOC = message.OpeningTimeLOC,
                        IntervalDuration = message.ClosingTimeLOC - message.OpeningTimeLOC,
                        IntervalType = ((int)IntervalType.Work),
                        EmployeeId = newEmployeeId,
                        Id = message.CompanyId,
                        Bookings = new()
                    };

                    dbcontext.ScheduleIntervals.Add(scheduleInterval);
                }
            }

            await dbcontext.SaveChangesAsync();
        }
    }
}
