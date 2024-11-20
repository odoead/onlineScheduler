using IdentityService.Models;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Shared.Events.User;

namespace IdentityService.Consumers
{
    public class UserEmailRequestedConsumer : IConsumer<UserEmailRequested>
    {
        private readonly UserManager<ApplicationUser> userManager;
        public UserEmailRequestedConsumer(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }
        public async Task Consume(ConsumeContext<UserEmailRequested> context)
        {
            var user = await userManager.FindByEmailAsync(context.Message.Email);
            if (user == null)
            {
                await context.RespondAsync<UserEmailRequestedNotFoundResult>(new UserEmailRequestedNotFoundResult { });
            }
            await context.RespondAsync<UserEmailRequestResult>(new UserEmailRequestResult { Email = user.Email, Id = user.Id, UserName = user.UserName, });

        }
    }
}
