using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.DB;
using NotificationService.Entities;
using NotificationService.Interfaces;
using Shared.Events.User;
using Shared.Exceptions.custom_exceptions;
using Shared.Paging;

namespace NotificationService.Services
{
    public class NotificationService : INotificationService
    {
        private Context dbcontext; IRequestClient<UserEmailRequested> _client;

        public NotificationService(Context context, IRequestClient<UserEmailRequested> client)
        {
            dbcontext = context;
            _client = client;
        }
        public async Task<PagedList<Notification>> GetNotifications(string workerEmail, int pageNumber, int pageSize)
        {
            var response = await _client.GetResponse<UserEmailRequestResult, UserEmailRequestedNotFoundResult>(new UserEmailRequested { Email = workerEmail });
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

            var query = dbcontext.Notifications.Where(n => n.RecieverId == workerId).OrderByDescending(n => n.Id);

            var count = await query.CountAsync();

            var notifications = await query.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize).ToListAsync();

            return PagedList<Notification>.ToPagedList(notifications, count, pageNumber, pageSize);

        }
    }
}
