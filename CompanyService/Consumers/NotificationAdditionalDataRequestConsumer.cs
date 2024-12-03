using CompanyService.DB;
using MassTransit;
using MassTransit.Initializers;
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

            var productName = await dbcontext.Products.FirstOrDefaultAsync(q => q.Id == mess.ProductId).Select(q => q.Name);
            dataPairs.Add("productname", productName);

            var company = await dbcontext.Products.FirstOrDefaultAsync(q => q.Id == mess.ProductId).Select(q => q.Company);
            dataPairs.Add("companyid", company.Id.ToString());
            dataPairs.Add("companyname", company.Name);

            await context.RespondAsync<NotificationAdditionalDataRequestResult>(new NotificationAdditionalDataRequestResult { Data = dataPairs });
        }
    }
}
