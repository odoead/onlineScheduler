using MassTransit;
using NotificationService.DB;
using NotificationService.Entities;
using Shared.Events.Booking;
using Shared.Events.Company;
using Shared.Events.User;

namespace NotificationService.Consumers
{
    public class BookingEditedConsumer : IConsumer<BookingEdited>
    {
        private readonly Context dbcontext;
        IRequestClient<UserIdRequested> userClient;
        IRequestClient<NotificationAdditionalDataRequested> client;
        public BookingEditedConsumer(Context context)
        {
            dbcontext = context;
        }
        public async Task Consume(ConsumeContext<BookingEdited> context)
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

            var data = await client.GetResponse<NotificationAdditionalDataResult>(new NotificationAdditionalDataRequested { ProductId = mess.ProductId });
            foreach (var keyval in data.Message.Data)
            {
                keyValues.Add(keyval.Key, keyval.Value);
            }

            var notification = new Notification
            {
                RecieverId = mess.WorkerId,
                Description = "booking edited successfuly",
                Service = ServiceType.Schedule,
                Title = "Booking edit",
                NotificationKeyValues = keyValues.Select(kv => new Data { Key = kv.Key, Value = kv.Value, }).ToList(),
            };
            await dbcontext.Notifications.AddAsync(notification);
            await dbcontext.SaveChangesAsync();
        }
    }
}
