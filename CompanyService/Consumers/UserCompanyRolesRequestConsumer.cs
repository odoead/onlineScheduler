using CompanyService.DB;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events.User;

namespace CompanyService.Consumers
{
    public class UserCompanyRolesRequestConsumer : IConsumer<UserCompanyRolesRequested>
    {
        private readonly Context dbcontext;
        public UserCompanyRolesRequestConsumer(Context context)
        {
            dbcontext = context;
        }

        public async Task Consume(ConsumeContext<UserCompanyRolesRequested> context)
        {

            Dictionary<string, string> roles = new();

            var workerCompanies = await dbcontext.СompanyWorkers.Where(q => q.WorkerId == context.Message.UserId).ToListAsync();
            foreach (var company in workerCompanies)
            {
                roles.Add("worker", company.CompanyID.ToString());
            }

            var ownerComp = await dbcontext.Companies.Where(q => q.OwnerId == context.Message.UserId).FirstOrDefaultAsync();
            if (ownerComp != null)
            {
                roles.Add("owner", ownerComp.Id.ToString());
            }
            await context.RespondAsync<UserCompanyRolesRequestResult>(new UserCompanyRolesRequestResult { Roles = roles });

        }
    }
}
