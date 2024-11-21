using Microsoft.EntityFrameworkCore;
using NotificationService.DB;
using NotificationService.Entities;
using NotificationService.Interfaces;
using Shared.Paging;

namespace NotificationService.Services
{
    public class NotificationService : INotificationService
    {
        private Context dbcontext;
        public NotificationService(Context context)
        {
            dbcontext= context;
        }
        public async Task<PagedList<Notification>> GetNotifications(string workerId, int pageNumber, int pageSize)
        {
            var query = dbcontext.Notifications.Where(n => n.RecieverId == workerId) .OrderByDescending(n => n.Id); 

            var count = await query.CountAsync();

            var notifications = await query.Skip((pageNumber - 1) * pageSize) 
                .Take(pageSize).ToListAsync();

            return PagedList<Notification>.ToPagedList(notifications, count, pageNumber, pageSize);

        }
    }
}
