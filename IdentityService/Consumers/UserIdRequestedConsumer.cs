using IdentityService.Models;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Shared.Events.User;

namespace IdentityService.Consumers
{
    public class UserIdRequestedConsumer : IConsumer<UserIdRequested>
    {
        private readonly UserManager<ApplicationUser> userManager;
        public UserIdRequestedConsumer(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }
        public async Task Consume(ConsumeContext<UserIdRequested> context)
        {
            var user = await userManager.FindByIdAsync(context.Message.Id);
            if (user == null)
            {
                await context.RespondAsync<UserIdRequestedNotFoundResult>(new UserEmailRequestedNotFoundResult { });
            }
            await context.RespondAsync<UserIdRequestResult>(new UserEmailRequestResult { Email = user.Email, Id = user.Id, UserName = user.UserName, });

        }
    }
}
