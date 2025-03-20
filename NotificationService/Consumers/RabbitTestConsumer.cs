using MassTransit;
using Shared.Events.Booking;
using Shared.Events.Company;
using static MassTransit.ValidationResultExtensions;

namespace NotificationService.Consumers
{
    public class RabbitTestConsumer : IConsumer<RabbitTestRequest>
    {
         
        public async Task Consume(ConsumeContext<RabbitTestRequest> context)
        {
            Console.WriteLine($"Received: {context.Message.val}");

            await context.RespondAsync(new RabbitTestRequestResult
            {
                returnVal = context.Message.val + " Test is ok"
            });
        }
    }
}
