using MassTransit;
using NotificationService.DB;
using NotificationService.Entities;
using Shared.Data;
using Shared.Events.Booking;
using Shared.Events.Company;
using Shared.Events.User;

namespace NotificationService.Consumers
{
    public class BookingCreatedConsumer : IConsumer<BookingCreated>
    {
        private readonly Context dbcontext;
        IRequestClient<UserIdRequested> userClient;
        IRequestClient<NotificationAdditionalDataRequested> client;

        public BookingCreatedConsumer(Context context)
        {
            dbcontext = context;
        }
        public async Task Consume(ConsumeContext<BookingCreated> context)
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

            var data = await client.GetResponse<NotificationAdditionalDataRequestResult>(new NotificationAdditionalDataRequested { ProductId = mess.ProductId });
            foreach (var keyval in data.Message.Data)
            {
                keyValues.Add(keyval.Key, keyval.Value);
            }
            var clientData = await userClient.GetResponse<UserIdRequestResult>(new UserIdRequested { Id = mess.ClientId });
            keyValues.Add("clientemail", clientData.Message.Email);
            keyValues.Add("clientname", clientData.Message.Email);
            keyValues.Add("status", BookingStatus.CREATED.ToString());

            var notification = new Notification
            {
                RecieverId = mess.WorkerId,
                Description = "booking request created successfuly",
                Service = ServiceType.SCHEDULE,
                Title = "Booking request",
                NotificationKeyValues = keyValues.Select(kv => new Data { Key = kv.Key, Value = kv.Value, }).ToList(),
            };
            await dbcontext.Notifications.AddAsync(notification);
            await dbcontext.SaveChangesAsync();
        }
    }
}
