using CompanyService.DB;
using CompanyService.Helpers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events.Company;

namespace CompanyService.Consumers
{
    public class GetCompanyTimeZoneConsumer : IConsumer<GetCompanyTimeZoneRequest>
    {
        private readonly Context dbcontext;
        public GetCompanyTimeZoneConsumer(Context context)
        {
            dbcontext = context;
        }

        public async Task Consume(ConsumeContext<GetCompanyTimeZoneRequest> ctx)
        {
            var company = await dbcontext.Companies.Include(q => q.Location).Include(q => q.Products).FirstOrDefaultAsync(q => q.Products.Any(p => p.Id == ctx.Message.ProductId));

            var tzInfo = TimezoneConverter.GetTimezoneFromLocation(company.Location.Coordinates.X, company.Location.Coordinates.Y);
            await ctx.RespondAsync(new GetCompanyTimeZoneResult { TimeZone = tzInfo });
        }
    }
}
