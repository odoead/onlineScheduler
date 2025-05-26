using MassTransit;
using NotificationService.Data;
using NotificationService.DB;
using Shared.Notification;

namespace NotificationService.Consumers
{
    public class NotificationReadConsumer : IConsumer<NotificationRead>
    {
        private readonly Context dbContext;

        public NotificationReadConsumer(Context context)
        {
            dbContext = context;
        }

        public async Task Consume(ConsumeContext<NotificationRead> context)
        {
            var message = context.Message;

            var notification = await dbContext.Notifications.FindAsync(message.NotificationId);

            if (notification != null)
            {
                notification.Status = NotificationStatus.READ;
                notification.ReadAtUTC = message.ReadAtUTC;

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
