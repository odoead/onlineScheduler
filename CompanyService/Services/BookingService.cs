using CompanyService.DB;
using CompanyService.DTO.Booking;
using CompanyService.DTO.Product;
using CompanyService.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events.Booking;
using Shared.Events.User;
using Shared.Exceptions.custom_exceptions;

namespace CompanyService.Services
{
    public class BookingService : IBookingService
    {
        private readonly Context dbcontext;
        private readonly IRequestClient<WorkerBookingsRequested> client;
        private readonly IRequestClient<UserEmailRequested> UserEmailclient;

        public BookingService(Context context, IRequestClient<WorkerBookingsRequested> client, IRequestClient<UserEmailRequested> userEmailclient)
        {
            dbcontext = context;
            client = client;
            UserEmailclient = userEmailclient;
        }
        public async Task<List<GetBookingDTO>> GetBookingsAsync(string workerEmail)
        {
            var response = await UserEmailclient.GetResponse<UserEmailRequestResult, UserEmailRequestedNotFoundResult>(new UserEmailRequested { Email = workerEmail });
            string clientId;
            switch (response)
            {
                case var r when r.Message is UserEmailRequestResult result:
                    clientId = result.Id;
                    break;
                case var r when r.Message is UserEmailRequestedNotFoundResult notFoundResult:
                    throw new BadRequestException("User with email " + workerEmail + " not found");
                default:
                    throw new InvalidOperationException("Unknown response type received.");
            }

            var bookings = await GetWorkersBookingsDataAsync(clientId);
            List<GetBookingDTO> bookingsDto = new List<GetBookingDTO>();

            foreach (var item in bookings.Bookings)
            {
                var product = await dbcontext.Products.Where(q => q.Id == item.ProductId).Select(q => new ProductMinDTO
                {
                    DurationTime = q.Duration,
                    Id = q.Id,
                    Title = q.Name
                }).FirstOrDefaultAsync();

                var company = await dbcontext.Companies.Where(q => q.Products.Any(p => p.Id == item.ProductId)).Select(q => q.Name).FirstOrDefaultAsync();

                bookingsDto.Add(new GetBookingDTO
                {
                    CustomerEmail = item.CustomerEmail,
                    CustomerName = item.CustomerName,
                    EndDateLOC = item.EndDateLOC,
                    Status = item.Status,
                    StartDateLOC = item.StartDateLOC,
                    Product = product,
                    CompanyName = company,
                    Id = item.Id
                });
            }
            return bookingsDto;
        }
        private async Task<WorkerBookingsRequestResult> GetWorkersBookingsDataAsync(string workerId)
        {
            var response = await client.GetResponse<WorkerBookingsRequestResult>(new WorkerBookingsRequested { workerId = workerId });
            return response.Message;
        }

    }
}
