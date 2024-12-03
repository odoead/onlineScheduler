using NotificationService.DTO;
using Shared.Paging;

namespace NotificationService.Interfaces
{
    public interface INotificationService
    {
        public Task<PagedList<NotificationDTO>> GetNotifications(string workerEmail, int pageNumber, int pageSize);
    }
}
