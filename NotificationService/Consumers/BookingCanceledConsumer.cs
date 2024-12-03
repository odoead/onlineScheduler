using MassTransit;
using NotificationService.DB;
using NotificationService.Entities;
using Shared.Data;
using Shared.Events.Booking;
using Shared.Events.Company;

namespace NotificationService.Consumers
{
    public class BookingCanceledConsumer : IConsumer<BookingCanceled>
    {
        private readonly Context dbcontext;
        IRequestClient<NotificationAdditionalDataRequested> client;

        public BookingCanceledConsumer(Context context)
        {
            dbcontext = context;
        }
        public async Task Consume(ConsumeContext<BookingCanceled> context)
        {
            var mess = context.Message;

            var keyValues = new Dictionary<string, string>();
            keyValues.Add("startdate", mess.StartDateLOC.ToString());
            if (mess.EndDateLOC.HasValue)
            {
                keyValues.Add("enddate", mess.EndDateLOC.Value.ToString());
            }
            keyValues.Add("bookingid", mess.BookingId.ToString());
            keyValues.Add("productid", mess.ProductId.ToString());
            keyValues.Add("status", BookingStatus.CANCELED.ToString());

            var data = await client.GetResponse<NotificationAdditionalDataRequestResult>(new NotificationAdditionalDataRequested { ProductId = mess.ProductId });
            foreach (var keyval in data.Message.Data)
            {
                keyValues.Add(keyval.Key, keyval.Value);
            }

            var notification = new Notification
            {
                RecieverId = mess.WorkerId,
                Description = "booking canceled",
                Service = ServiceType.SCHEDULE,
                Title = "Booking canceled",
                NotificationKeyValues = keyValues.Select(kv => new Data { Key = kv.Key, Value = kv.Value, }).ToList(),
            };
            await dbcontext.Notifications.AddAsync(notification);
            await dbcontext.SaveChangesAsync();
        }
    }
}
