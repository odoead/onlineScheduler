using NotificationService.Entities;
using Shared.Paging;

namespace NotificationService.Interfaces
{
    public interface INotificationService
    {
        public Task<PagedList<Notification>> GetNotifications(string workerEmail, int pageNumber, int pageSize);
    }
}
