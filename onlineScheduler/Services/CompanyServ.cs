using CompanyService.DB;
using CompanyService.DTO;
using CompanyService.Entities;
using CompanyService.Helpers;
using CompanyService.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.Exceptions.custom_exceptions;
using Shared.Messages.Company;

namespace CompanyService.Services
{
    public class CompanyServ : ICompanyService
    {
        private readonly Context _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public CompanyServ(Context context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }


        public async Task<int> AddCompanyAsync(CreateCompanyDTO companyDTO)
        {
            var location = new Location
            {
                Coordinates = new NpgsqlTypes.NpgsqlPoint { X = companyDTO.Longitude, Y = companyDTO.Latitude }
            };
            var timeZoneOffset = TimezoneConverter.GetTimezoneFromLocation(companyDTO.Longitude, companyDTO.Latitude).BaseUtcOffset;
            Company company = companyDTO.CompanyType switch
            {
                (int)CompanyType.Personal => new PersonalCompany
                {
                    Name = companyDTO.Name,
                    Description = companyDTO.Description,
                    OpeningTimeLOC = companyDTO.OpeningTimeLOC,
                    ClosingTimeLOC = companyDTO.ClosingTimeLOC,
                    CompanyType = CompanyType.Personal,
                    OwnerId = companyDTO.OwnerId,
                    WorkerId = companyDTO.OwnerId,
                    Location = location,
                    TimeZoneFromUTCOffset = timeZoneOffset,
                    Products = new List<Product>(),
                    WorkingDays = companyDTO.WorkingDays.Select(d => (DayOfTheWeek)d).ToList(),
                },
                (int)CompanyType.Shared => new SharedCompany
                {
                    Name = companyDTO.Name,
                    Description = companyDTO.Description,
                    OpeningTimeLOC = companyDTO.OpeningTimeLOC,
                    ClosingTimeLOC = companyDTO.ClosingTimeLOC,
                    CompanyType = CompanyType.Shared,
                    OwnerId = companyDTO.OwnerId,
                    Location = location,
                    TimeZoneFromUTCOffset = timeZoneOffset,
                    Workers = await _context.Users
                        .Where(u => companyDTO.EmployeeIds.Contains(u.Id))
                        .Select(u => new CompanyWorkers { WorkerId = u.Id })
                        .ToListAsync(),
                    Products = new List<Product>(),
                    WorkingDays = companyDTO.WorkingDays.Select(d => (DayOfTheWeek)d).ToList(),

                },
                _ => throw new BadRequestException("Invalid company type.")
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            await _publishEndpoint.Publish(new CompanyCreated
            {
                CompanyId = company.Id,
                CompanyName = company.Name,
                OwnerId = company.OwnerId,
                ClosingTimeLOC = company.ClosingTimeLOC,
                CompanyType = companyDTO.CompanyType,
                EmployeeIds = companyDTO.EmployeeIds,
                OpeningTimeLOC = company.OpeningTimeLOC,
                WorkingDays = companyDTO.WorkingDays.Select(d => (DayOfTheWeek)d).ToList()
            });

            return company.Id;
        }

        public async Task<bool> DeleteCompanyAsync(int companyId)
        {
            var company = await _context.Companies.FindAsync(companyId);
            if (company == null) return false;

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            await _publishEndpoint.Publish(new CompanyDeleted
            {
                CompanyId = companyId,
            });

            return true;
        }

        public async Task<bool> UpdateCompanyEmployeesAsync(int companyId, List<string> employeeIds)
        {
            var company = await _context.Companies.FindAsync(companyId);
            if (company == null) return false;

            var employees = await _context.Users.Where(u => employeeIds.Contains(u.Id)).ToListAsync();
            if (company is SharedCompany sharedCompany)
            {
                sharedCompany.Workers = employees.Select(e => new CompanyWorkers { CompanyID = company.Id, WorkerId = e.Id }).ToList();
                _context.Companies.Update(sharedCompany);
                await _context.SaveChangesAsync();
            }

            await _publishEndpoint.Publish(new UpdatedCompanyEmployees
            {
                CompanyId = companyId,
                EmployeeIds = employeeIds,
                ClosingTimeLOC = company.ClosingTimeLOC,
                OpeningTimeLOC = company.OpeningTimeLOC,
                WorkingDays = company.WorkingDays,
            });

            return true;
        }


        public async Task<GetCompanyDTO> GetCompany(int companyId)
        {
            var company = await _context.Companies.Include(q => q.Owner)
                .Include(q => ((SharedCompany)q).Workers).ThenInclude(q => q.Worker)
                .Include(q => ((PersonalCompany)q).Worker)
                .Where(q => q.Id == companyId).FirstOrDefaultAsync();

            var dto = new GetCompanyDTO
            {
                Name = company.Name,
                Description = company.Description,
                OpeningTimeLOC = company.OpeningTimeLOC,
                ClosingTimeLOC = company.ClosingTimeLOC,
                CompanyType = company.CompanyType,
                OwnerId = company.OwnerId,
                OwnerName = company.Owner.FullName,
                Employees = new(),
                Longitude = company.Location.Coordinates.X,
                Latitude = company.Location.Coordinates.Y,
            };
            dto.Products = company.Products.Select(p => new ProductDTO { DurationTime = p.DurationTime, Id = p.Id, Title = p.Title }).ToList();
            if (company is SharedCompany sharedCompany)
            {
                dto.Employees = sharedCompany.Workers.Select(w => new UserDTO { FullName = w.Worker.FullName, Id = w.WorkerId, UserType = w.Worker.UserType }).ToList();
            }
            else if (company is PersonalCompany personalCompany)
            {
                dto.Employees = new List<UserDTO> { new UserDTO { FullName = personalCompany.Worker.FullName, Id = personalCompany.WorkerId, UserType = personalCompany.Worker.UserType } };
            }

            return dto;
        }

    }
}
