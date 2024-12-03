using CompanyService.DB;
using CompanyService.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events.Booking;

namespace CompanyService.Consumers
{
    /// <summary>
    /// Check if new booking can be added 
    /// </summary>
    public class IsValidBookingTimeRequestConsumer : IConsumer<IsValidBookingTimeRequested>
    {
        private readonly Context dbcontext;
        private readonly IBookingValidationService validationService;
        public IsValidBookingTimeRequestConsumer(Context context, IBookingValidationService validationService)
        {
            dbcontext = context;
            this.validationService = validationService;
        }
        public async Task Consume(ConsumeContext<IsValidBookingTimeRequested> context)
        {
            var message = context.Message;
            var companyId = await dbcontext.Companies.Where(company => company.Products.Any(p => p.Id == message.ProductId)).Select(q => q.Id).FirstOrDefaultAsync();
            var result = new IsValidBookingTimeRequestResult { IsValid = await validationService.IsValidBookingTime(message.StartDateLOC, message.EndDateLOC, companyId, message.WorkerId) };
            await context.RespondAsync<IsValidBookingTimeRequestResult>(result);

        }
    }
}
