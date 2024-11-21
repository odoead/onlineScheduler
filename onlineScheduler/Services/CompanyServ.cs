using CompanyService.DB;
using CompanyService.DTO;
using CompanyService.Entities;
using CompanyService.Helpers;
using CompanyService.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.Events.User;
using Shared.Exceptions.custom_exceptions;

namespace CompanyService.Services
{
    public class CompanyServ : ICompanyService
    {
        private readonly Context dbcontext;
        private readonly IPublishEndpoint _publishEndpoint;
        IRequestClient<UserEmailRequested> _client;
        private readonly IBookingValidationService bookingValidator;

        public CompanyServ(Context context, IPublishEndpoint publishEndpoint, IRequestClient<UserEmailRequested> client, IBookingValidationService bookingValidator)
        {
            dbcontext = context;
            _publishEndpoint = publishEndpoint;
            _client = client;
            this.bookingValidator = bookingValidator;
        }


        public async Task<int> AddCompanyAsync(string Name, string Description, TimeSpan OpeningTimeLOC, TimeSpan ClosingTimeLOC, int _CompanyType,
            List<int> WorkingDays, double Latitude, double Longitude, string ownerEmail)
        {

            var location = new Location
            {
                Coordinates = new NpgsqlTypes.NpgsqlPoint { X = Longitude, Y = Latitude }
            };
            var timeZoneOffset = TimezoneConverter.GetTimezoneFromLocation(Longitude, Latitude).BaseUtcOffset;

            var ownerData = await GetUserData(ownerEmail);
            await CheckAndAddWorkers(new List<UserEmailRequestResult> { ownerData });


            Company company = _CompanyType switch
            {
                (int)CompanyType.Personal => new PersonalCompany
                {
                    Name = Name,
                    Description = Description,
                    OpeningTimeLOC = OpeningTimeLOC,
                    ClosingTimeLOC = ClosingTimeLOC,
                    OwnerId = ownerData.Id,
                    WorkerId = ownerData.Id,
                    Location = location,
                    TimeZoneFromUTCOffset = timeZoneOffset,
                    Products = new List<Product>(),
                    WorkingDays = WorkingDays.Select(d => (DayOfTheWeek)d).ToList(),
                },
                (int)CompanyType.Shared => new SharedCompany
                {
                    Name = Name,
                    Description = Description,
                    OpeningTimeLOC = OpeningTimeLOC,
                    ClosingTimeLOC = ClosingTimeLOC,
                    OwnerId = ownerData.Id,
                    Location = location,
                    TimeZoneFromUTCOffset = timeZoneOffset,
                    Workers = new List<CompanyWorker>
                {
                    new CompanyWorker { WorkerId = ownerData.Id }
                },
                    Products = new List<Product>(),
                    WorkingDays = WorkingDays.Select(d => (DayOfTheWeek)d).ToList(),
                },
                _ => throw new BadRequestException("Invalid company type.")
            };
            await dbcontext.Companies.AddAsync(company);
            await dbcontext.SaveChangesAsync();

            var newSchedules = CreateScheduleForWorkers(new List<string> { ownerData.Id }, company.Id, OpeningTimeLOC, ClosingTimeLOC,
                 WorkingDays.Select(d => (DayOfTheWeek)d).ToList());

            await dbcontext.ScheduleIntervals.AddRangeAsync(newSchedules);
            await dbcontext.SaveChangesAsync();
            /*await _publishEndpoint.Publish(new CompanyCreated
            {
                CompanyId = company.Id,
                CompanyName = company.Name,
                OwnerId = company.OwnerId,
                ClosingTimeLOC = company.ClosingTimeLOC,
                CompanyType =  CompanyType,
                EmployeeIds =  EmployeeIds,
                OpeningTimeLOC = company.OpeningTimeLOC,
                WorkingDays =  WorkingDays.Select(d => (DayOfTheWeek)d).ToList()
            });*/

            return company.Id;
        }

        public async Task<bool> DeleteCompanyAsync(int companyId)
        {

            var company = await dbcontext.Companies.FindAsync(companyId) ?? throw new BadRequestException("Company doesnt exist. Id: " + companyId);

            // Check for active bookings
            if (await bookingValidator.HasActiveBookingsCompany(companyId))
            {
                throw new BadRequestException("Cannot remove company with active bookings");
            }

            dbcontext.Companies.Remove(company);
            await dbcontext.SaveChangesAsync();

            /*await _publishEndpoint.Publish(new CompanyDeleted
            {
                CompanyId = companyId,
            });*/

            return true;
        }

        public async Task AddEmployeesToCompany(int companyId, List<string> UserEmails)
        {
            var company = await dbcontext.SharedCompanies.Include(q => q.Workers).Include(c => c.ScheduleIntervals)
                .FirstOrDefaultAsync(q => q.Id == companyId) ?? throw new BadRequestException("Company doesnt exist. Id: " + companyId);

            List<UserEmailRequestResult> responseUsers = new();
            foreach (var email in UserEmails)
            {
                var response = await GetUserData(email);
                responseUsers.Add(response);
            }

            if (!responseUsers.Select(u => u.Id).Except(company.Workers.Select(w => w.WorkerId)).ToList().Any())//workers already added to company
            { return; }
            await CheckAndAddWorkers(responseUsers);

            var newWorkerIds = responseUsers.Select(u => u.Id).Except(company.Workers.Select(w => w.WorkerId)).ToList();
            foreach (var id in newWorkerIds)
            {
                company.Workers.Add(new CompanyWorker { CompanyID = company.Id, WorkerId = id, });
            }

            var schedules = CreateScheduleForWorkers(newWorkerIds, companyId, company.OpeningTimeLOC, company.OpeningTimeLOC, company.WorkingDays);


            await dbcontext.ScheduleIntervals.AddRangeAsync(schedules);
            await dbcontext.SaveChangesAsync();
            /*await _publishEndpoint.Publish(new UpdatedCompanyEmployees
            {
                CompanyId = companyId,
                EmployeeIds = responseUsers.Select(q=>q.Id).ToList(),
                ClosingTimeLOC = company.ClosingTimeLOC,
                OpeningTimeLOC = company.OpeningTimeLOC,
                WorkingDays = company.WorkingDays,
            });*/
        }

        public async Task<bool> RemoveEmployeeFromCompany(int companyId, string workerId)
        {
            var company = await dbcontext.SharedCompanies
                .Include(c => c.Workers)
                .FirstOrDefaultAsync(c => c.Id == companyId)
                ?? throw new BadRequestException("Company doesn't exist. Id: " + companyId);

            if (company.OwnerId == workerId)
            {
                throw new BadRequestException("Cannot remove the company owner. Comany: " + company.Name + " |Owner: " + workerId);
            }

            var workerAssignment = company.Workers
                .FirstOrDefault(w => w.WorkerId == workerId)
                ?? throw new BadRequestException("Worker not found in company");

            // Check for active bookings
            if (await bookingValidator.HasActiveBookingsWorker(workerId))
            {
                throw new BadRequestException("Cannot remove worker with active bookings");
            }

            company.Workers.Remove(workerAssignment);

            // Remove schedule intervals
            var scheduleIntervals = await dbcontext.ScheduleIntervals
                .Where(si => si.CompanyId == companyId && si.WorkerId == workerId)
                .ToListAsync();
            dbcontext.ScheduleIntervals.RemoveRange(scheduleIntervals);
            await dbcontext.SaveChangesAsync();

            return true;
        }

        public async Task<GetCompanyDTO> GetCompany(int companyId)
        {
            var company = await dbcontext.Companies.Include(q => q.Owner).Include(q => q.Location).Include(q=>q.Products)
                .Include(q => ((SharedCompany)q).Workers).ThenInclude(q => q.Worker)
                .Include(q => ((PersonalCompany)q).Worker)
                .Where(q => q.Id == companyId).FirstOrDefaultAsync();

            var dto = new GetCompanyDTO
            {
                Name = company.Name,
                Description = company.Description,
                OpeningTimeLOC = company.OpeningTimeLOC,
                ClosingTimeLOC = company.ClosingTimeLOC,
                OwnerId = company.OwnerId,
                OwnerName = company.Owner.FullName,
                Longitude = company.Location.Coordinates.X,
                Latitude = company.Location.Coordinates.Y,
            };
            dto.Products = company.Products.Select(p => new ProductDTO { DurationTime = p.Duration, Id = p.Id, Title = p.Name,  }).ToList();
            if (company is SharedCompany sharedCompany)
            {
                dto.CompanyType= CompanyType.Shared;
                dto.Workers = sharedCompany.Workers.Select(w => new WorkerMinDTO { FullName = w.Worker.FullName, Id = w.WorkerId, }).ToList();
            }
            else if (company is PersonalCompany personalCompany)
            {
                dto.CompanyType = CompanyType.Personal;
                dto.Workers = new List<WorkerMinDTO> { new WorkerMinDTO { FullName = personalCompany.Worker.FullName, Id = personalCompany.WorkerId } };
            }

            return dto;
        }

        private async Task CheckAndAddWorkers(List<UserEmailRequestResult> responseUsers)
        {
            var existingWorkers = await dbcontext.Workers
                .Where(w => responseUsers.Select(r => r.Id).Contains(w.IdentityServiceId))
                .ToListAsync();

            foreach (var user in responseUsers)
            {
                if (!existingWorkers.Any(w => w.IdentityServiceId == user.Id))
                {
                    var newWorker = new Worker
                    {
                        IdentityServiceId = user.Id,
                        FullName = user.UserName,
                    };
                    await dbcontext.Workers.AddAsync(newWorker);
                }
            }
            await dbcontext.SaveChangesAsync();
        }

        private List<ScheduleInterval> CreateScheduleForWorkers(List<string> workerIds, int companyId, TimeSpan startTime, TimeSpan finishTime, List<DayOfTheWeek> weekday)
        {
            List<ScheduleInterval> scheduleIntervals = new();
            foreach (var id in workerIds)
            {
                foreach (var day in weekday)
                {
                    scheduleIntervals.Add(new ScheduleInterval
                    { WorkerId = id, CompanyId = companyId, StartTimeLOC = startTime, WeekDay = (int)day, IntervalType = IntervalType.Work, FinishTimeLOC = finishTime, });

                }
            }
            return scheduleIntervals;
        }

        private async Task<UserEmailRequestResult> GetUserData(string email)
        {
            var response = await _client.GetResponse<UserEmailRequestResult, UserEmailRequestedNotFoundResult>(new UserEmailRequested { Email = email });
            switch (response)
            {
                case var r when r.Message is UserEmailRequestResult result:
                    return result;

                case var r when r.Message is UserEmailRequestedNotFoundResult notFoundResult:
                    throw new BadRequestException("User with email " + email + " not found");

                default:
                    throw new InvalidOperationException("Unknown response type received.");
            }

        }

    }
}
