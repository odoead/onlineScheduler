using BookingService.DB;
using BookingService.Entities;
using MassTransit;
using Shared.Events.Product;

namespace BookingService.Consumers
{
    public class CreateProductConsumer : IConsumer<ProductForCompanyCreated>
    {
        private readonly Context dbcontext;
        public CreateProductConsumer(Context context)
        {
            dbcontext = context;
        }
        public async Task Consume(ConsumeContext<ProductForCompanyCreated> context)
        {
            var message = context.Message;
            await dbcontext.Products.AddAsync(new Product { Id = message.ProductID, Name = message.Name, Duration = message.DurationTime });
            await dbcontext.SaveChangesAsync();
        }
    }
}
