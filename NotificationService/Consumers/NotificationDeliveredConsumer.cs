using MassTransit;
using NotificationService.Data;
using NotificationService.DB;
using Shared.Notification;

namespace NotificationService.Consumers
{
    public class NotificationDeliveredConsumer : IConsumer<NotificationDelivered>
    {
        private readonly Context dbContext;

        public NotificationDeliveredConsumer(Context context)
        {
            dbContext = context;
        }

        public async Task Consume(ConsumeContext<NotificationDelivered> context)
        {
            var message = context.Message;
            var notification = await dbContext.Notifications.FindAsync(message.NotificationId);

            if (notification != null)
            {
                notification.Status = NotificationStatus.DELIVERED;
                notification.DeliveredAtUTC = message.DeliveredAt;

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
