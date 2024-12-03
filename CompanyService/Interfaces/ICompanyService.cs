using CompanyService.DTO.Company;

namespace CompanyService.Interfaces
{
    public interface ICompanyService
    {
        public Task<int> AddCompanyAsync(string Name, string Description, TimeSpan OpeningTimeLOC, TimeSpan ClosingTimeLOC, int CompanyType,
            List<int> WorkingDays, double Latitude, double Longitude, string ownerEmail);
        public Task<bool> DeleteCompanyAsync(int companyId);
        public Task AddEmployeesToCompany(int companyId, List<string> UserEmails);
        public Task<bool> RemoveEmployeeFromCompany(int companyId, string workerId);
        public Task<GetCompanyDTO> GetCompany(int companyId);

    }
}
