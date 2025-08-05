using MassTransit;
using NotificationService.Data;
using NotificationService.DB;
using NotificationService.Entities;
using NotificationService.Interfaces;
using Shared.Data;
using Shared.Events.Booking;
using Shared.Events.Company;
using Shared.Events.User;
using Shared.Exceptions.custom_exceptions;

namespace NotificationService.Consumers
{
    public class BookingCreatedConsumer : IConsumer<BookingCreated>
    {
        private readonly Context dbContext;
        private readonly IRequestClient<UserIdRequested> userClient;
        private readonly IRequestClient<NotificationAdditionalDataRequested> additionalDataClient;
        private readonly INotificationService notificationService;

        public BookingCreatedConsumer(Context dbContext, IRequestClient<UserIdRequested> userClient, IRequestClient<NotificationAdditionalDataRequested> additionalDataClient, INotificationService notificationService)
        {
            this.dbContext = dbContext;
            this.userClient = userClient;
            this.additionalDataClient = additionalDataClient;
            this.notificationService = notificationService;
        }

        public async Task Consume(ConsumeContext<BookingCreated> context)
        {
            var message = context.Message;
            var keyValues = await BuildKeyValuesAsync(message);

            await HandleWorkerNotificationAsync(message, keyValues);
            await HandleClientNotificationAsync(message, keyValues);
            //Booking reminders already exists
            // await CreateBookingRemindersAsync(message.BookingId, message.BookingsClientId, message.BookingsWorkerId, message.BookingStartDateUTC, keyValues);
        }

        private async Task<Dictionary<string, string>> BuildKeyValuesAsync(BookingCreated message)
        {
            var keyValues = new Dictionary<string, string>();

            var additionalDataResponse = await additionalDataClient.GetResponse<NotificationAdditionalDataRequestResult>(
                new NotificationAdditionalDataRequested { ProductId = message.BookingProductId, });
            var additionalData = additionalDataResponse.Message.Data;
            foreach (var keyValue in additionalData)
            {
                keyValues[keyValue.Key] = keyValue.Value;
            }

            var clientDataResponse = await userClient.GetResponse<UserIdRequestResult>(new UserIdRequested { Id = message.BookingsClientId });
            if (clientDataResponse.Message is UserIdRequestedNotFoundResult)
            {
                throw new BadRequestException($"Client with id {message.BookingsClientId} not found");
            }

            var workerDataResponse = await userClient.GetResponse<UserIdRequestResult, UserIdRequestedNotFoundResult>(new UserIdRequested { Id = message.BookingsWorkerId });

            string workerName = workerDataResponse.Message switch
            {
                UserIdRequestResult result => result.UserName,
                UserIdRequestedNotFoundResult => throw new BadRequestException($"Worker with id {message.BookingsWorkerId} not found"),
                _ => string.Empty
            };

            keyValues["startdateloc"] = message.BookingStartDateLOC.ToString();
            keyValues.Add("startdateutc", message.BookingStartDateUTC.ToString());
            if (message.BookingEndDateLOC.HasValue)
            {
                keyValues["enddateloc"] = message.BookingEndDateLOC.Value.ToString();
            }
            keyValues["bookingid"] = message.BookingId.ToString();
            keyValues["productid"] = message.BookingProductId.ToString();
            keyValues["clientid"] = message.BookingsClientId;
            keyValues["workerid"] = message.BookingsWorkerId;
            keyValues["clientemail"] = clientDataResponse.Message.Email;
            keyValues["clientname"] = clientDataResponse.Message.UserName;
            keyValues["status"] = BookingStatus.CREATED.ToString();
            keyValues["workername"] = workerName;

            return keyValues;
        }

        private async Task HandleWorkerNotificationAsync(BookingCreated message, Dictionary<string, string> keyValues)
        {
            if (keyValues.TryGetValue("DoesScheduleNotifyWorkerOnIncomingBooking", out var notifyWorkerSetting) && !bool.Parse(notifyWorkerSetting))
            {
                return;
            }

            var workerNotification = new Notification
            {
                RecieverId = message.BookingsWorkerId,
                Service = ServiceType.SCHEDULE,
                Type = NotificationType.BOOKING_CREATED,
                Status = NotificationStatus.CREATED,
                Title = $"New Booking Request for {keyValues.GetValueOrDefault("productname", "service")}",
                Description = $"New booking request has been created by {keyValues["clientname"]}",
                CreatedAtUTC = DateTime.UtcNow,
                NotificationKeyValues = keyValues.Select(kv => new KVData { Key = kv.Key, Value = kv.Value }).ToList(),
            };
            await dbContext.Notifications.AddAsync(workerNotification);
            await dbContext.SaveChangesAsync();
            await notificationService.StartDelivery(workerNotification.Id);
        }

        private async Task HandleClientNotificationAsync(BookingCreated message, Dictionary<string, string> keyValues)
        {
            if (keyValues.TryGetValue("DoesScheduleNotifyClientOnIncomingBooking", out var notifyClientSetting) && !bool.Parse(notifyClientSetting))
            {
                return;
            }

            if (keyValues.TryGetValue("TimeBeforeBookingStartWhenNotScheduleNotifyClientInMinutes_OnBookingCreated", out var minTimeSetting) &&
                int.TryParse(minTimeSetting, out var minimumNotifyTime))
            {
                var timeUntilBooking = message.BookingStartDateUTC - DateTime.UtcNow;
                if (timeUntilBooking.TotalMinutes < minimumNotifyTime)
                {
                    return;
                }
            }

            var clientNotification = new Notification
            {
                RecieverId = message.BookingsClientId,
                Service = ServiceType.SCHEDULE,
                Type = NotificationType.BOOKING_CREATED,
                Status = NotificationStatus.CREATED,
                Title = $"Booking Request Sent to {keyValues.GetValueOrDefault("companyname", "company")}",
                Description = $"Your booking request for {keyValues.GetValueOrDefault("productname", "service")} has been sent",
                CreatedAtUTC = DateTime.UtcNow,
                NotificationKeyValues = keyValues.Select(kv => new KVData { Key = kv.Key, Value = kv.Value }).ToList(),
            };

            await dbContext.Notifications.AddAsync(clientNotification);
            await dbContext.SaveChangesAsync();
            await notificationService.StartDelivery(clientNotification.Id);
        }

        private async Task CreateBookingRemindersAsync(int bookingId, string clientId, string workerId, DateTime bookingStartDateUTC, Dictionary<string, string> companySettings)
        {
            async Task AddReminder(string receiverId, int minutesBefore, NotificationType type)
            {
                var reminderTime = bookingStartDateUTC.AddMinutes(-minutesBefore);
                if (reminderTime <= DateTime.UtcNow) return;

                var scheduledNotification = new ScheduledNotification
                {
                    RecieverId = receiverId,
                    BookingId = bookingId,
                    ScheduledDateForUTC = reminderTime,
                    Type = type,
                    IsProcessed = false,
                    CreatedAtUTC = DateTime.UtcNow,
                };
                await dbContext.ScheduledNotifications.AddAsync(scheduledNotification);
            }

            if (!string.IsNullOrEmpty(clientId))
            {
                if (companySettings.TryGetValue("TimeBeforeBookingStartWhenNotifyInMinutes_OnBookingIncoming_ClientLong", out var clientLongReminderSetting) &&
                    int.TryParse(clientLongReminderSetting, out var clientLongReminderMinutes))
                {
                    await AddReminder(clientId, clientLongReminderMinutes, NotificationType.APPOINTMENT_REMINDER);
                }

                if (companySettings.TryGetValue("TimeBeforeBookingStartWhenNotifyInMinutes_OnBookingIncoming", out var clientShortReminderSetting) &&
                    int.TryParse(clientShortReminderSetting, out var clientShortReminderMinutes))
                {
                    await AddReminder(clientId, clientShortReminderMinutes, NotificationType.APPOINTMENT_REMINDER);
                }
            }

            if (!string.IsNullOrEmpty(workerId) &&
                companySettings.TryGetValue("TimeBeforeBookingStartWhenNotifyInMinutes_OnBookingIncoming", out var workerReminderSetting) &&
                int.TryParse(workerReminderSetting, out var workerReminderMinutes))
            {
                await AddReminder(workerId, workerReminderMinutes, NotificationType.APPOINTMENT_REMINDER);
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
