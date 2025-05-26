using Hangfire;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.DB;
using NotificationService.DTO;
using NotificationService.Entities;
using NotificationService.Interfaces;
using Shared.Events.User;
using Shared.Exceptions.custom_exceptions;
using Shared.Notification;
using Shared.Paging;

namespace NotificationService.Services
{
    public class NotificationService : INotificationService
    {
        private Context dbcontext;
        private IRequestClient<UserEmailRequested> client;
        private IPublishEndpoint publishEndpoint;
        private IRecurringJobManager recurringJobManager;
        IConfiguration configuration;
        public NotificationService(Context context, IRecurringJobManager recurringJobManager, IRequestClient<UserEmailRequested> userClient, IPublishEndpoint publishEndpoint, IConfiguration configuration)
        {
            dbcontext = context;
            client = userClient;
            this.publishEndpoint = publishEndpoint;
            this.configuration = configuration;
            this.recurringJobManager = recurringJobManager;

            SetupRecurringJobs();
        }

        private void SetupRecurringJobs()
        {
            recurringJobManager.AddOrUpdate("process-scheduled-notifications", () => ProcessScheduledNotifications(), Cron.MinuteInterval(5));
            recurringJobManager.AddOrUpdate("send-appointment-reminders", () => SendUpcomingAppointmentReminders(), Cron.MinuteInterval(10));
        }

        public async Task<PagedList<NotificationDTO>> GetNotifications(string workerEmail, int pageNumber, int pageSize)
        {
            var response = await client.GetResponse<UserEmailRequestResult, UserEmailRequestedNotFoundResult>(new UserEmailRequested { Email = workerEmail });
            string workerId;
            switch (response)
            {
                case var r when r.Message is UserEmailRequestResult result:
                    workerId = result.Id;
                    break;
                case var r when r.Message is UserEmailRequestedNotFoundResult notFoundResult:
                    throw new BadRequestException("User with email " + workerEmail + " not found");
                default:
                    throw new InvalidOperationException("Unknown response type received.");
            }

            var query = dbcontext.Notifications.Include(q => q.NotificationKeyValues).Where(n => n.RecieverId == workerId).OrderByDescending(n => n.Id);

            var count = await query.CountAsync();

            var notifications = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var notificationDtos = notifications.Select(n => new NotificationDTO
            {
                Id = n.Id,
                RecieverId = n.RecieverId,
                Service = n.Service.ToString(),
                Type = n.Type.ToString(),
                Status = n.Status.ToString(),
                Title = n.Title,
                Description = n.Description,
                CreatedAtUTC = n.CreatedAtUTC,
                DeliveredAtUTC = n.DeliveredAtUTC,
                ReadAtUTC = n.ReadAtUTC,
                NotificationKeyValues = n.NotificationKeyValues.ToDictionary(q => q.Key, q => q.Value)
            }).ToList();

            return PagedList<NotificationDTO>.ToPagedList(notificationDtos, count, pageNumber, pageSize);
        }

        public async Task StartDelivery(int notificationId)
        {
            var notification = await dbcontext.Notifications.Include(n => n.NotificationKeyValues).FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification != null && notification.Status == NotificationStatus.CREATED)
            {
                notification.Status = NotificationStatus.INDELIVERY;
                await dbcontext.SaveChangesAsync();

                // Simulate delivery  ..... 
                //
                //
                await publishEndpoint.Publish(new NotificationDelivered
                {
                    NotificationId = notificationId,
                    DeliveredAt = DateTime.UtcNow
                });
            }
        }

        public async Task MarkAsRead(int notificationId)
        {
            await publishEndpoint.Publish(new NotificationRead
            {
                NotificationId = notificationId,
                ReadAtUTC = DateTime.UtcNow
            });
        }

        public async Task ProcessScheduledNotifications()
        {
            var currentTime = DateTime.UtcNow;
            var scheduledNotifications = await dbcontext.ScheduledNotifications.Where(n => n.ScheduledDateForUTC <= currentTime && !n.IsProcessed).ToListAsync();

            foreach (var scheduledNotification in scheduledNotifications)
            {
                switch (scheduledNotification.Type)
                {
                    case NotificationType.APPOINTMENT_REMINDER:
                        await CreateAppointmentReminderNotification(scheduledNotification);
                        break;
                    default:
                        break;
                }
                scheduledNotification.IsProcessed = true;
            }

            await dbcontext.SaveChangesAsync();
        }

        private async Task CreateAppointmentReminderNotification(ScheduledNotification scheduledNotification)
        {
            var keyValues = new Dictionary<string, string>();
            keyValues.Add("bookingid", scheduledNotification.BookingId.ToString());

            var bookingNotification = await dbcontext.Notifications.Include(n => n.NotificationKeyValues)
                .Where(n => n.NotificationKeyValues.Any(kv => kv.Key == "bookingid" && kv.Value == scheduledNotification.BookingId.ToString())).FirstOrDefaultAsync();

            if (bookingNotification != null)
            {
                foreach (var kv in bookingNotification.NotificationKeyValues)
                {
                    if (!keyValues.ContainsKey(kv.Key))
                    {
                        keyValues.Add(kv.Key, kv.Value);
                    }
                }

                //calculate time until the appointment
                DateTime startDate = DateTime.UtcNow;
                var startDateKvUTC = bookingNotification.NotificationKeyValues.FirstOrDefault(kv => kv.Key == "startdateutc");

                if (startDateKvUTC != null && DateTime.TryParse(startDateKvUTC.Value, out startDate))
                {
                    TimeSpan timeUntil = startDate - DateTime.UtcNow;
                    string timeDescription;
                    //string 
                    if (timeUntil.TotalHours >= 1)
                    {
                        timeDescription = $"in {Math.Floor(timeUntil.TotalHours)} hour(s)";
                    }
                    else
                    {
                        timeDescription = $"in {Math.Floor(timeUntil.TotalMinutes)} minutes";
                    }

                    var notification = new Notification
                    {
                        RecieverId = scheduledNotification.RecieverId,
                        Service = ServiceType.SCHEDULE,
                        Type = NotificationType.APPOINTMENT_REMINDER,
                        Status = NotificationStatus.CREATED,
                        Title = "Appointment Reminder",
                        Description = $"Reminder: Your appointment starts {timeDescription}",
                        CreatedAtUTC = scheduledNotification.ScheduledDateForUTC,
                        NotificationKeyValues = keyValues.Select(kv => new KVData
                        {
                            Key = kv.Key,
                            Value = kv.Value,
                        }).ToList(),
                    };

                    await dbcontext.Notifications.AddAsync(notification);
                    await dbcontext.SaveChangesAsync();

                    await StartDelivery(notification.Id);
                }
            }
        }

        ///batch process all near future notifications
        public async Task SendUpcomingAppointmentReminders()
        {
            var currentTime = DateTime.UtcNow;
            var reminderWindow = currentTime.AddMinutes(30); // Check appointments in the next 30 minutes

            var upcomingNotifications = await dbcontext.Notifications.Include(n => n.NotificationKeyValues).Where(n => n.Type == NotificationType.BOOKING_CONFIRMED || n.Type == NotificationType.BOOKING_CREATED
                && n.NotificationKeyValues.Any(kv => kv.Key == "startdateutc"))
                .SelectMany(n => n.NotificationKeyValues.Where(kv => kv.Key == "startdateutc")).ToListAsync();

            var upcomingBookingIds = upcomingNotifications.Where(kv => DateTime.TryParse(kv.Value, out var startDate) && startDate > currentTime && startDate <= reminderWindow)
                .Select(kv => kv.Notification.NotificationKeyValues.FirstOrDefault(n => n.Key == "bookingid")?.Value).Where(id => id != null).Distinct();

            foreach (var bookingId in upcomingBookingIds)
            {
                if (int.TryParse(bookingId, out var id))
                {
                    var existingReminder = await dbcontext.Notifications.Include(n => n.NotificationKeyValues)
                        .Where(n => n.Type == NotificationType.APPOINTMENT_REMINDER)
                        .Where(n => n.NotificationKeyValues.Any(kv => kv.Key == "bookingid" && kv.Value == bookingId))
                        .Where(n => n.CreatedAtUTC >= currentTime.AddMinutes(-30))
                        .FirstOrDefaultAsync();

                    if (existingReminder != null)
                    {
                        await StartDelivery(existingReminder.Id);
                    }
                }
            }
        }
    }
}
