using CompanyService.DB;
using CompanyService.DTO.Booking;
using CompanyService.DTO.Product;
using CompanyService.Entities;
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
        private readonly IRequestClient<WorkerBookingsRequested> WorkerBookingsClient;
        private readonly IRequestClient<UserEmailRequested> UserEmailclient;
        private readonly IRequestClient<GetClientBookingsRequested> ClientBookingsClient;
        private readonly IRequestClient<BookingStatisticsRequest> BookingStatClient;
        private readonly IConfiguration configuration;

        public BookingService(Context context, IRequestClient<GetClientBookingsRequested> clientBookings, IRequestClient<WorkerBookingsRequested> client,
            IRequestClient<UserEmailRequested> userEmailclient, IConfiguration configuration, IRequestClient<BookingStatisticsRequest> bookingStatClient)
        {
            dbcontext = context;
            WorkerBookingsClient = client;
            UserEmailclient = userEmailclient;
            ClientBookingsClient = clientBookings;
            this.configuration = configuration;
            BookingStatClient = bookingStatClient;
        }
        public async Task<List<GetBookingDTO_Worker>> GetWorkerBookingsAsync(string workerEmail)
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
            List<GetBookingDTO_Worker> bookingsDto = new List<GetBookingDTO_Worker>();

            foreach (var item in bookings.Bookings)
            {
                var product = await dbcontext.Products.Where(q => q.Id == item.ProductId).Select(q => new ProductMinDTO
                {
                    DurationTime = q.Duration,
                    Id = q.Id,
                    Title = q.Name
                }).FirstOrDefaultAsync();

                var company = await dbcontext.Companies.Where(q => q.Products.Any(p => p.Id == item.ProductId)).Select(q => q.Name).FirstOrDefaultAsync();

                bookingsDto.Add(new GetBookingDTO_Worker
                {
                    CustomerEmail = item.CustomerEmail,
                    CustomerName = item.CustomerName,
                    EndDateLOC = item.EndDateLOC,
                    Status = item.Status,
                    StartDateLOC = item.StartDateLOC,
                    Product = product,
                    CompanyName = company,
                    Id = item.BookingId,
                });
            }
            return bookingsDto;
        }
        private async Task<WorkerBookingsRequestResult> GetWorkersBookingsDataAsync(string workerId)
        {
            var response = await WorkerBookingsClient.GetResponse<WorkerBookingsRequestResult>(new WorkerBookingsRequested { workerId = workerId });
            return response.Message;
        }


        public async Task<List<GetBookingDTO_Client>> GetClientBookingsAsync(string clientEmail)
        {
            var response = await UserEmailclient.GetResponse<UserEmailRequestResult, UserEmailRequestedNotFoundResult>(
                new UserEmailRequested { Email = clientEmail });

            string? clientId;
            switch (response)
            {
                case var r when r.Message is UserEmailRequestResult result:
                    clientId = result.Id;
                    break;
                case var r when r.Message is UserEmailRequestedNotFoundResult notFoundResult:
                    throw new BadRequestException("User with email " + clientEmail + " not found");
                default:
                    throw new InvalidOperationException("Unknown response type received.");
            }

            var clientBookingsResponse = await ClientBookingsClient.GetResponse<GetClientBookingsRequestResult>(new GetClientBookingsRequested { clientId = clientId });
            var clientBookings = clientBookingsResponse.Message;

            var bookings = await dbcontext.Bookings.Include(q => q.Worker)
                .Include(b => b.Product)
                .ThenInclude(p => p.Company)
                .Where(b => clientBookings.Bookings.Select(q => q.BookingId).ToList().Contains(b.BookingServiceId))
                .OrderBy(b => b.StartDateLOC)
                .ToListAsync();

            List<GetBookingDTO_Client> bookingsDto = new List<GetBookingDTO_Client>();
            foreach (var item in bookings)
            {
                var product = new ProductMinDTO
                {
                    DurationTime = item.Product.Duration,
                    Id = item.Product.Id,
                    Title = item.Product.Name,
                };

                bookingsDto.Add(new GetBookingDTO_Client
                {
                    EmployeeId = item.Worker.IdentityServiceId,
                    EmployeeName = item.Worker.FullName,
                    EndDateLOC = item.EndDateLOC,
                    Status = clientBookings.Bookings.FirstOrDefault(q => q.BookingId == item.BookingServiceId)?.Status ?? string.Empty,
                    StartDateLOC = item.StartDateLOC,
                    Product = product,
                    CompanyName = item.Product.Company.Name,
                    Id = item.Id,
                });
            }

            return bookingsDto;
        }

        public async Task<BookingStatisticsDTO> GetCompanyBookingsStatisticsAsync(int companyId, DateTime? startDate, DateTime? endDate)
        {
            var company = await dbcontext.Companies.Include(c => (c as SharedCompany).Workers)
                .FirstOrDefaultAsync(c => c.Id == companyId) ?? throw new BadRequestException("Company not found");

            var effectiveStartDate = startDate ?? DateTime.MinValue;
            var effectiveEndDate = endDate ?? DateTime.MaxValue;

            var bookings = await dbcontext.Bookings.Include(b => b.Product).ThenInclude(p => p.Company).Include(b => b.Worker)
                .Where(b => b.Product.CompanyId == companyId && b.StartDateLOC >= effectiveStartDate && b.StartDateLOC <= effectiveEndDate).ToListAsync();

            // Get all worker IDs for the company
            var workerIds = (company is SharedCompany sharedCompany && sharedCompany.Workers != null)
                ? sharedCompany.Workers.Select(w => w.WorkerId).ToList() : new List<string>();

            // Request statistics from BookingService
            var response = await WorkerBookingsClient.GetResponse<BookingStatisticsRequestResult>(new BookingStatisticsRequest
            {
                WorkerIds = workerIds,
                StartDate = effectiveStartDate,
                EndDate = effectiveEndDate
            });

            var result = response.Message;

            #region Booking Statistics
            var bookingsByProduct = bookings.GroupBy(b => b.ProductId).Select(g => new BookingProductStatDTO
            {
                ProductId = g.Key,
                ProductName = g.First().Product.Name,
                BookingCount = g.Count()
            }).OrderByDescending(p => p.BookingCount).ToList();

            var bookingsByWorker = bookings.GroupBy(b => b.WorkerId).Select(g => new BookingWorkerStatDTO
            {
                WorkerId = g.Key,
                WorkerName = g.First().Worker.FullName,
                BookingCount = g.Count()
            }).OrderByDescending(w => w.BookingCount).ToList();

            var bookingsByDay = bookings.GroupBy(b => b.StartDateLOC.Date).Select(g => new BookingDayStatDTO
            {
                Date = g.Key,
                BookingCount = g.Count()
            }).OrderBy(d => d.Date).ToList();

            #endregion
            return new BookingStatisticsDTO
            {
                TotalBookings = result.TotalBookings,
                CompletedBookings = result.CompletedBookings,
                CancelledBookings = result.CancelledBookings,
                PendingBookings = result.PendingBookings,
                BookingsByProduct = bookingsByProduct,
                BookingsByWorker = bookingsByWorker,
                BookingsByDay = bookingsByDay,
                StartDate = effectiveStartDate,
                EndDate = effectiveEndDate,
            };
        }


    }
}
