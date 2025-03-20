using CompanyService.DB;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events.Company;

namespace CompanyService.Consumers
{
    public class NotificationAdditionalDataRequestConsumer : IConsumer<NotificationAdditionalDataRequested>
    {
        private readonly Context dbcontext;
        public NotificationAdditionalDataRequestConsumer(Context context)
        {
            dbcontext = context;
        }
        public async Task Consume(ConsumeContext<NotificationAdditionalDataRequested> context)
        {
            var mess = context.Message;
            Dictionary<string, string> dataPairs = new Dictionary<string, string>();

            var product = await dbcontext.Products.Include(q => q.Company).FirstOrDefaultAsync(q => q.Id == mess.ProductId);
            dataPairs.Add("productname", product.Name);

            dataPairs.Add("companyid", product.Company.Id.ToString());
            dataPairs.Add("companyname", product.Company.Name);

            await context.RespondAsync<NotificationAdditionalDataRequestResult>(new NotificationAdditionalDataRequestResult { Data = dataPairs });
        }
    }
}
