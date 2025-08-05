using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.DB;
using NotificationService.Entities;
using NotificationService.Interfaces;
using Shared.Events.Booking;
using Shared.Events.Company;
using Shared.Events.User;

namespace NotificationService.Consumers
{
    public class BookingEditCreatedConsumer : IConsumer<BookingEditCreatedRequest>
    {
        private readonly Context dbContext;
        private readonly IRequestClient<NotificationAdditionalDataRequested> client;
        private readonly INotificationService notificationService;
        private readonly IRequestClient<UserIdRequested> userClient;

        public BookingEditCreatedConsumer(Context context, IRequestClient<NotificationAdditionalDataRequested> additionalDataClient, IRequestClient<UserIdRequested> userClient, INotificationService notificationService)
        {
            dbContext = context;
            this.client = additionalDataClient;
            this.notificationService = notificationService;
            this.userClient = userClient;
        }

        public async Task Consume(ConsumeContext<BookingEditCreatedRequest> context)
        {
            var mess = context.Message;
            var keyValues = await BuildKeyValuesAsync(mess);

            var booking = await dbContext.Notifications
                .Where(n => n.NotificationKeyValues.Any(kv => kv.Key == "bookingid" && kv.Value == mess.BookingId.ToString()))
                .Include(n => n.NotificationKeyValues)
                .FirstOrDefaultAsync();

            var (clientId, clientName, workerName) = await GetClientAndWorkerDetailsAsync(booking, mess.WorkerId, keyValues);

            await CreateWorkerNotificationAsync(mess, keyValues, workerName);

            if (ShouldNotifyClient(keyValues) && !string.IsNullOrEmpty(clientId))
            {
                await CreateClientNotificationAsync(mess, keyValues, clientId);
            }

            await UpdateBookingReminders(mess.BookingId, clientId, mess.WorkerId, mess.StartDateLOC, keyValues);
        }

        private async Task<Dictionary<string, string>> BuildKeyValuesAsync(BookingEditCreatedRequest mess)
        {
            var keyValues = new Dictionary<string, string>
            {
                { "startdateloc", mess.StartDateLOC.ToString() },
                { "startdateutc", mess.StartDateUTC.ToString() },
                { "bookingid", mess.BookingId.ToString() },
                { "productid", mess.ProductId.ToString() }
            };

            if (mess.EndDateLOC.HasValue)
            {
                keyValues.Add("enddateloc", mess.EndDateLOC.Value.ToString());
            }

            var data = await client.GetResponse<NotificationAdditionalDataRequestResult>(new NotificationAdditionalDataRequested { ProductId = mess.ProductId });
            foreach (var keyval in data.Message.Data)
            {
                keyValues[keyval.Key] = keyval.Value;
            }

            return keyValues;
        }

        private async Task<(string clientId, string clientName, string workerName)> GetClientAndWorkerDetailsAsync(Notification booking, string workerId, Dictionary<string, string> keyValues)
        {
            string clientId = null, clientName = null, workerName = null;

            if (booking != null)
            {
                clientId = booking.NotificationKeyValues.FirstOrDefault(kv => kv.Key == "clientid")?.Value;
                if (!string.IsNullOrEmpty(clientId))
                {
                    keyValues["clientid"] = clientId;
                    var clientDataResponse = await userClient.GetResponse<UserIdRequestResult, UserIdRequestedNotFoundResult>(new UserIdRequested { Id = clientId });
                    if (clientDataResponse.Message is UserIdRequestResult result)
                    {
                        clientName = result.UserName;
                        keyValues["clientname"] = clientName;
                        keyValues["clientemail"] = result.Email;
                    }
                }

                var workerDataResponse = await userClient.GetResponse<UserIdRequestResult, UserIdRequestedNotFoundResult>(new UserIdRequested { Id = workerId });
                if (workerDataResponse.Message is UserIdRequestResult workerResult)
                {
                    workerName = workerResult.UserName;
                    keyValues["workername"] = workerName;
                }
            }

            return (clientId, clientName, workerName);
        }

        private async Task CreateWorkerNotificationAsync(BookingEditCreatedRequest mess, Dictionary<string, string> keyValues, string workerName)
        {
            var workerNotification = new Notification
            {
                RecieverId = mess.WorkerId,
                Service = ServiceType.SCHEDULE,
                Type = NotificationType.BOOKING_EDITED,
                Status = NotificationStatus.CREATED,
                Title = $"Booking Updated for {keyValues.GetValueOrDefault("productname", "service")}",
                Description = $"Booking details have been updated for {mess.StartDateLOC}",
                CreatedAtUTC = DateTime.UtcNow,
                NotificationKeyValues = keyValues.Select(kv => new KVData { Key = kv.Key, Value = kv.Value }).ToList()
            };

            await dbContext.Notifications.AddAsync(workerNotification);
            await dbContext.SaveChangesAsync();
            await notificationService.StartDelivery(workerNotification.Id);
        }

        private async Task CreateClientNotificationAsync(BookingEditCreatedRequest mess, Dictionary<string, string> keyValues, string clientId)
        {
            var clientNotification = new Notification
            {
                RecieverId = clientId,
                Service = ServiceType.SCHEDULE,
                Type = NotificationType.BOOKING_EDITED,
                Status = NotificationStatus.CREATED,
                Title = $"Your Booking Updated at {keyValues.GetValueOrDefault("companyname", "company")}",
                Description = $"Your appointment for {keyValues.GetValueOrDefault("productname", "service")} on {mess.StartDateLOC} has been updated",
                CreatedAtUTC = DateTime.UtcNow,
                NotificationKeyValues = keyValues.Select(kv => new KVData { Key = kv.Key, Value = kv.Value }).ToList()
            };

            await dbContext.Notifications.AddAsync(clientNotification);
            await dbContext.SaveChangesAsync();
            await notificationService.StartDelivery(clientNotification.Id);
        }

        private bool ShouldNotifyClient(Dictionary<string, string> keyValues)
        {
            return keyValues.TryGetValue("DoesSendClientNotificationOnBookingEdited", out var notifyClientSetting) && bool.Parse(notifyClientSetting);
        }

        private async Task UpdateBookingReminders(int bookingId, string clientId, string workerId, DateTime bookingStartDateUTC, Dictionary<string, string> companySettings)
        {
            var existingNotifications = await dbContext.ScheduledNotifications.Where(n => n.BookingId == bookingId && !n.IsProcessed)
                .ToListAsync();

            dbContext.ScheduledNotifications.RemoveRange(existingNotifications);

            int reminderMinutes = GetReminderMinutes(companySettings, "TimeBeforeBookingStartWhenNotifyInMinutes_OnBookingIncoming", 60);
            int clientLongReminderMinutes = GetReminderMinutes(companySettings, "TimeBeforeBookingStartWhenNotifyInMinutes_OnBookingIncoming_ClientLong", 24 * 60);

            if (!string.IsNullOrEmpty(clientId))
            {
                await CreateScheduledNotificationAsync(clientId, bookingId, bookingStartDateUTC, clientLongReminderMinutes, NotificationType.APPOINTMENT_REMINDER, companySettings);
                await CreateScheduledNotificationAsync(clientId, bookingId, bookingStartDateUTC, reminderMinutes, NotificationType.APPOINTMENT_REMINDER, companySettings);
            }

            await CreateScheduledNotificationAsync(workerId, bookingId, bookingStartDateUTC, reminderMinutes, NotificationType.APPOINTMENT_REMINDER, companySettings);
            await dbContext.SaveChangesAsync();
        }

        private int GetReminderMinutes(Dictionary<string, string> settings, string key, int defaultValue)
        {
            return settings.TryGetValue(key, out var value) && int.TryParse(value, out var parsedValue) ? parsedValue : defaultValue;
        }

        private async Task CreateScheduledNotificationAsync(string receiverId, int bookingId, DateTime bookingStartDateUTC, int reminderMinutes, NotificationType type, Dictionary<string, string> companySettings)
        {
            var reminderTimeUTC = bookingStartDateUTC.AddMinutes(-reminderMinutes);
            var timeUntilBookingStart = (bookingStartDateUTC - DateTime.UtcNow).TotalMinutes;

            // Check if the client's booking is too close to create notification for it
            if (companySettings.TryGetValue("TimeBeforeBookingStartWhenNotScheduleNotifyClientInMinutes_OnBookingCreated", out var minNotifySetting) && int.TryParse(minNotifySetting, out var minNotify)
                && timeUntilBookingStart < minNotify)
            {
                return;
            }

            if (reminderTimeUTC > DateTime.UtcNow)
            {
                var scheduledNotification = new ScheduledNotification
                {
                    RecieverId = receiverId,
                    BookingId = bookingId,
                    ScheduledDateForUTC = reminderTimeUTC,
                    Type = type,
                    IsProcessed = false,
                    CreatedAtUTC = DateTime.UtcNow
                };
                await dbContext.ScheduledNotifications.AddAsync(scheduledNotification);
            }
        }
    }
}


