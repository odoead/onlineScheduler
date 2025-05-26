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
    public class BookingConfirmedConsumer : IConsumer<BookingConfirmed>
    {
        private readonly Context dbcontext;
        private readonly IRequestClient<NotificationAdditionalDataRequested> client;
        private readonly INotificationService notificationService;
        private readonly IRequestClient<UserIdRequested> userClient;

        public BookingConfirmedConsumer(Context context, IRequestClient<NotificationAdditionalDataRequested> additionalDataClient, INotificationService notificationService, IRequestClient<UserIdRequested> userClient)
        {
            dbcontext = context;
            client = additionalDataClient;
            this.notificationService = notificationService;
            this.userClient = userClient;
        }

        public async Task Consume(ConsumeContext<BookingConfirmed> context)
        {
            var mess = context.Message;

            var keyValues = new Dictionary<string, string>();
            keyValues.Add("startdateloc", mess.BookingStartDateLOC.ToString());
            keyValues.Add("startdateutc", mess.BookingStartDateUTC.ToString());

            if (mess.BookingEndDateLOC.HasValue)
            {
                keyValues.Add("enddateloc", mess.BookingEndDateLOC.Value.ToString());
            }
            keyValues.Add("workerid", mess.WorkerId.ToString());
            keyValues.Add("bookingid", mess.BookingId.ToString());
            keyValues.Add("productid", mess.ProductId.ToString());
            keyValues.Add("status", BookingStatus.CONFIRMED.ToString());

            var dataResponse = await client.GetResponse<NotificationAdditionalDataRequestResult>(
                        new NotificationAdditionalDataRequested { ProductId = mess.ProductId, });
            foreach (var keyval in dataResponse.Message.Data)
            {
                keyValues.Add(keyval.Key, keyval.Value);
            }

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
                    var clientDataResponse = await userClient.GetResponse<UserIdRequestResult, UserIdRequestedNotFoundResult>(new UserIdRequested { Id = clientId });
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
                var workerDataResponse = await userClient.GetResponse<UserIdRequestResult, UserIdRequestedNotFoundResult>(new UserIdRequested { Id = workerId });
                switch (workerDataResponse)
                {
                    case var r when r.Message is UserIdRequestResult result:
                        workerName = result.UserName;
                        keyValues["workername"] = workerName;
                        break;
                }
            }

            // Create worker notification about confirmation
            var workerNotification = new Notification
            {
                RecieverId = workerId,
                Service = ServiceType.SCHEDULE,
                Type = NotificationType.BOOKING_CONFIRMED,
                Status = NotificationStatus.CREATED,
                Title = $"Booking Confirmed for {keyValues.GetValueOrDefault("productname", "service")}",
                Description = $"Booking has been confirmed for {mess.BookingStartDateLOC}",
                CreatedAtUTC = DateTime.UtcNow,
                NotificationKeyValues = keyValues.Select(q => new KVData
                {
                    Key = q.Key,
                    Value = q.Value
                }).ToList(),
            };
            await dbcontext.Notifications.AddAsync(workerNotification);
            await dbcontext.SaveChangesAsync();
            await notificationService.StartDelivery(workerNotification.Id);

            var additionalDataResponse = dataResponse.Message.Data;
            // Create client notification  
            bool doesNotifyClient = false;
            if (additionalDataResponse.TryGetValue("DoesSendClientNotificationOnBookingConfirmed", out var notifyClientSetting))
            {
                doesNotifyClient = bool.Parse(notifyClientSetting);
            }

            if (doesNotifyClient && !string.IsNullOrEmpty(clientId))
            {
                var clientNotification = new Notification
                {
                    RecieverId = clientId,
                    Service = ServiceType.SCHEDULE,
                    Type = NotificationType.BOOKING_CONFIRMED,
                    Status = NotificationStatus.CREATED,
                    Title = $"Your Booking is Confirmed at {keyValues.GetValueOrDefault("companyname", "company")}",
                    Description = $"Your appointment for {keyValues.GetValueOrDefault("productname", "service")} on {mess.BookingStartDateLOC} has been confirmed",
                    CreatedAtUTC = DateTime.UtcNow,
                    NotificationKeyValues = keyValues.Select(q => new KVData
                    {
                        Key = q.Key,
                        Value = q.Value
                    }).ToList(),
                };
                await dbcontext.Notifications.AddAsync(clientNotification);
                await dbcontext.SaveChangesAsync();
                await notificationService.StartDelivery(clientNotification.Id);


            }
            await UpdateBookingReminders(mess.BookingId, clientId, workerId, mess.BookingStartDateUTC, additionalDataResponse);
        }

        private async Task UpdateBookingReminders(int bookingId, string clientId, string workerId, DateTime bookingStartDateUTC, Dictionary<string, string> additionalCompanyData)
        {
            // Remove existing scheduled notifications
            var existingNotifications = await dbcontext.ScheduledNotifications
                .Where(n => n.BookingId == bookingId && !n.IsProcessed)
                .ToListAsync();

            dbcontext.ScheduledNotifications.RemoveRange(existingNotifications);

            // Check for custom reminder time in settings
            int reminderMinutes = 60; // Default to 1 hour
            if (additionalCompanyData.TryGetValue("TimeBeforeBookingStartWhenNotifyInMinutes_OnBookingIncoming", out var reminderMinutesSetting))
            {
                if (int.TryParse(reminderMinutesSetting, out var parsedMinutes))
                {
                    reminderMinutes = parsedMinutes;
                }
            }

            // Check for client long reminder time setting
            int clientLongReminderMinutes = 24 * 60; // Default 24 hours
            if (additionalCompanyData.TryGetValue("TimeBeforeBookingStartWhenNotifyInMinutes_OnBookingIncoming_ClientLong", out var clientLongReminderSetting))
            {
                if (int.TryParse(clientLongReminderSetting, out var parsedMinutes))
                {
                    clientLongReminderMinutes = parsedMinutes;
                }
            }

            // Create client reminders if client exists
            if (!string.IsNullOrEmpty(clientId))
            {
                var clientLongReminderTimeUTC = bookingStartDateUTC.AddMinutes(-clientLongReminderMinutes);
                if (clientLongReminderTimeUTC > DateTime.UtcNow &&
                    ShouldCreateScheduledNotification_OnTimeBeforeBookingStart(bookingId, workerId, bookingStartDateUTC, additionalCompanyData))
                {
                    var scheduledClientLongNotification = new ScheduledNotification
                    {
                        RecieverId = clientId,
                        BookingId = bookingId,
                        ScheduledDateForUTC = clientLongReminderTimeUTC,
                        Type = NotificationType.APPOINTMENT_REMINDER,
                        IsProcessed = false,
                        CreatedAtUTC = DateTime.UtcNow,
                    };
                    await dbcontext.ScheduledNotifications.AddAsync(scheduledClientLongNotification);
                }

                // Client short reminder 
                var clientShortReminderTime = bookingStartDateUTC.AddMinutes(-reminderMinutes);
                if (clientShortReminderTime > DateTime.UtcNow &&
                    ShouldCreateScheduledNotification_OnTimeBeforeBookingStart(bookingId, workerId, bookingStartDateUTC, additionalCompanyData))
                {
                    var scheduledClientNotification = new ScheduledNotification
                    {
                        RecieverId = clientId,
                        BookingId = bookingId,
                        ScheduledDateForUTC = clientShortReminderTime,
                        Type = NotificationType.APPOINTMENT_REMINDER,
                        IsProcessed = false,
                        CreatedAtUTC = DateTime.UtcNow,
                    };
                    await dbcontext.ScheduledNotifications.AddAsync(scheduledClientNotification);
                }
            }

            // Worker reminder
            var workerReminderTimeUTC = bookingStartDateUTC.AddMinutes(-reminderMinutes);
            if (workerReminderTimeUTC > DateTime.UtcNow &&
                ShouldCreateScheduledNotification_OnTimeBeforeBookingStart(bookingId, workerId, bookingStartDateUTC, additionalCompanyData))
            {
                var scheduledWorkerNotification = new ScheduledNotification
                {
                    RecieverId = workerId,
                    BookingId = bookingId,
                    ScheduledDateForUTC = workerReminderTimeUTC,
                    Type = NotificationType.APPOINTMENT_REMINDER,
                    IsProcessed = false,
                    CreatedAtUTC = DateTime.UtcNow,
                };
                await dbcontext.ScheduledNotifications.AddAsync(scheduledWorkerNotification);
            }

            await dbcontext.SaveChangesAsync();
        }
        private bool ShouldCreateScheduledNotification_OnTimeBeforeBookingStart(int bookingId, string receiverId, DateTime bookingStartDateUTC, Dictionary<string, string> companySettings)
        {
            var timeUntilBookingStart = (bookingStartDateUTC - DateTime.UtcNow).TotalMinutes;

            // Check if the event is too close to send a scheduled notification  
            if (companySettings.TryGetValue("TimeBeforeBookingStartWhenNotScheduleNotifyClientInMinutes_OnBookingCreated", out var minNotifySetting)
                && int.TryParse(minNotifySetting, out var minNotify)
                && timeUntilBookingStart < minNotify)
            {
                return false;
            }
            return true;
        }
    }
}

