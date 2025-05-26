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

            var product = await dbcontext.Products.Include(q => q.Company).ThenInclude(q => q.Settings).FirstOrDefaultAsync(q => q.Id == mess.ProductId);
            dataPairs.Add("productname", product.Name);

            dataPairs.Add("companyid", product.Company.Id.ToString());
            dataPairs.Add("companyname", product.Company.Name);

            dataPairs.Add("DoesSendWorkerNotificationOnBookingCreated", product.Company.Settings.DoesSendWorkerNotificationOnBookingCreated.ToString());
            dataPairs.Add("TimeBeforeBookingStartWhenNotScheduleNotifyClientInMinutes_OnBookingCreated", product.Company.Settings.TimeBeforeBookingStartWhenNotScheduleNotifyClientInMinutes_OnBookingCreated.ToString());
            dataPairs.Add("DoesSendClientNotificationOnBookingConfirmed", product.Company.Settings.DoesSendClientNotificationOnBookingConfirmed.ToString());
            dataPairs.Add("DoesSendClientNotificationOnBookingEdited", product.Company.Settings.DoesSendClientNotificationOnBookingEdited.ToString());
            dataPairs.Add("DoesSendWorkerNotificationOnBookingCanceled", product.Company.Settings.DoesSendWorkerNotificationOnBookingCanceled.ToString());
            dataPairs.Add("DoesSendClientNotificationOnBookingCanceled", product.Company.Settings.DoesSendClientNotificationOnBookingCanceled.ToString());
            dataPairs.Add("DoesScheduleNotifyClientOnIncomingBooking", product.Company.Settings.DoesScheduleNotifyClientOnIncomingBooking.ToString());
            dataPairs.Add("DoesScheduleNotifyWorkerOnIncomingBooking", product.Company.Settings.DoesScheduleNotifyWorkerOnIncomingBooking.ToString());
            dataPairs.Add("TimeBeforeBookingStartWhenScheduleNotifyInMinutes_OnBookingIncoming", product.Company.Settings.TimeBeforeBookingStartWhenScheduleNotifyInMinutes_OnBookingIncoming.ToString());
            dataPairs.Add("TimeBeforeBookingStartWhenScheduleNotifyInMinutes_OnBookingIncoming_ClientLong", product.Company.Settings.TimeBeforeBookingStartWhenScheduleNotifyInMinutes_OnBookingIncoming_ClientLong.ToString());


            await context.RespondAsync<NotificationAdditionalDataRequestResult>(new NotificationAdditionalDataRequestResult { Data = dataPairs });
        }
    }
}
