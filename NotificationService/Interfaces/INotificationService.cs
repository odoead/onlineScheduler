using NotificationService.DTO;
using Shared.Paging;

namespace NotificationService.Interfaces
{
    public interface INotificationService
    {
        Task<PagedList<NotificationDTO>> GetNotifications(string workerEmail, int pageNumber, int pageSize);
        Task StartDelivery(int notificationId);
        Task MarkAsRead(int notificationId);
        Task ProcessScheduledNotifications();
        Task SendUpcomingAppointmentReminders();
    }
}
