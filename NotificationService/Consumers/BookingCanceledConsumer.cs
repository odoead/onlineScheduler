using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.DB;
using NotificationService.Entities;
using NotificationService.Interfaces;
using Shared.Data;
using Shared.Events.Booking;
using Shared.Events.Company;
using Shared.Events.User;

namespace NotificationService.Consumers
{
    public class BookingCanceledConsumer : IConsumer<BookingCanceled>
    {
        private readonly Context dbcontext;
        private readonly INotificationService notificationService;
        private readonly IRequestClient<NotificationAdditionalDataRequested> client;
        private readonly IRequestClient<UserIdRequested> userClient;

        public BookingCanceledConsumer(Context context, IRequestClient<UserIdRequested> userClient, IRequestClient<NotificationAdditionalDataRequested> additionalDataClient, INotificationService notificationService)
        {
            dbcontext = context;
            this.client = additionalDataClient;
            this.notificationService = notificationService;
            this.userClient = userClient;
        }

        public async Task Consume(ConsumeContext<BookingCanceled> context)
        {
            var mess = context.Message;

            var keyValues = new Dictionary<string, string>();
            keyValues.Add("startdateloc", mess.StartDateLOC.ToString());
            keyValues.Add("startdateutc", mess.StartDateUTC.ToString());
            if (mess.EndDateLOC.HasValue)
            {
                keyValues.Add("enddateloc", mess.EndDateLOC.Value.ToString());
            }
            keyValues.Add("bookingid", mess.BookingId.ToString());
            keyValues.Add("productid", mess.ProductId.ToString());
            keyValues.Add("originalstatus", mess.OriginalStatus.ToString());
            keyValues.Add("status", BookingStatus.CANCELED.ToString());
            keyValues.Add("workerid", mess.WorkerId.ToString());

            var data = await client.GetResponse<NotificationAdditionalDataRequestResult>(
new NotificationAdditionalDataRequested { ProductId = mess.ProductId });
            foreach (var keyval in data.Message.Data)
            {
                keyValues.Add(keyval.Key, keyval.Value);
            }
            var additionalData = data.Message.Data;

            var booking = await dbcontext.Notifications.Where(n => n.NotificationKeyValues.Any(kv => kv.Key == "bookingid" && kv.Value == mess.BookingId.ToString()))
                .Include(n => n.NotificationKeyValues).FirstOrDefaultAsync();

            string clientId = null;
            string clientName = null;
            string workerId = mess.WorkerId;
            string workerName = null;

            if (booking != null)
            {
                var clientIdKv = booking.NotificationKeyValues.FirstOrDefault(kv => kv.Key == "clientid");
                if (clientIdKv != null)
                {
                    clientId = clientIdKv.Value;
                    keyValues["clientid"] = clientId;

                    // Get client name
                    var clientDataResponse = await userClient.GetResponse<UserIdRequestResult, UserIdRequestedNotFoundResult>(
                        new UserIdRequested { Id = clientId, });

                    switch (clientDataResponse)
                    {
                        case var r when r.Message is UserIdRequestResult result:
                            clientName = result.UserName;
                            keyValues["clientname"] = clientName;
                            keyValues["clientemail"] = result.Email;
                            break;
                    }
                }

                // Get worker name
                var workerDataResponse = await userClient.GetResponse<UserIdRequestResult, UserIdRequestedNotFoundResult>(
                    new UserIdRequested { Id = workerId, });

                switch (workerDataResponse)
                {
                    case var r when r.Message is UserIdRequestResult result:
                        workerName = result.UserName;
                        keyValues["workername"] = workerName;
                        break;
                }
            }

            bool doesNotifyWorker = false;
            if (additionalData.TryGetValue("DoesSendWorkerNotificationOnBookingCanceled", out var notifyWorkerSetting))
            {
                doesNotifyWorker = bool.Parse(notifyWorkerSetting);
            }

            // Create worker notification if enabled
            if (doesNotifyWorker)
            {
                var workerNotification = new Notification
                {
                    RecieverId = workerId,
                    Service = ServiceType.SCHEDULE,
                    Type = NotificationType.BOOKING_CANCELED,
                    Status = NotificationStatus.CREATED,
                    Title = $"Booking Canceled for {keyValues.GetValueOrDefault("productname", "service")}",
                    Description = $"A booking for {mess.StartDateLOC} has been canceled",
                    CreatedAtUTC = DateTime.UtcNow,
                    NotificationKeyValues = keyValues.Select(kv => new KVData
                    {
                        Key = kv.Key,
                        Value = kv.Value
                    }).ToList()
                };

                await dbcontext.Notifications.AddAsync(workerNotification);
                await dbcontext.SaveChangesAsync();
                await notificationService.StartDelivery(workerNotification.Id);
            }

            // Check company settings for client notification
            bool doesNotifyClient = false;
            if (additionalData.TryGetValue("DoesSendClientNotificationOnBookingCanceled", out var notifyClientSetting))
            {
                doesNotifyClient = bool.Parse(notifyClientSetting);
            }
            if (doesNotifyClient && !string.IsNullOrEmpty(clientId))
            {
                var clientNotification = new Notification
                {
                    RecieverId = clientId,
                    Service = ServiceType.SCHEDULE,
                    Type = NotificationType.BOOKING_CANCELED,
                    Status = NotificationStatus.CREATED,
                    Title = $"Your Booking is Canceled at {keyValues.GetValueOrDefault("companyname", "company")}",
                    Description = $"Your appointment for {keyValues.GetValueOrDefault("productname", "service")} on {mess.StartDateLOC} has been canceled",
                    CreatedAtUTC = DateTime.UtcNow,
                    NotificationKeyValues = keyValues.Select(kv => new KVData
                    {
                        Key = kv.Key,
                        Value = kv.Value
                    }).ToList(),
                };

                await dbcontext.Notifications.AddAsync(clientNotification);
                await dbcontext.SaveChangesAsync();
                await notificationService.StartDelivery(clientNotification.Id);
            }

            // Remove all scheduled reminders for this booking
            var scheduledNotifications = await dbcontext.ScheduledNotifications.Where(n => n.BookingId == mess.BookingId && !n.IsProcessed).ToListAsync();
            dbcontext.ScheduledNotifications.RemoveRange(scheduledNotifications);
            await dbcontext.SaveChangesAsync();
        }
    }
}

