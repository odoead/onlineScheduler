using CompanyService.DTO;

namespace CompanyService.Interfaces
{
    public interface ICompanyService
    {
        public Task<int> AddCompanyAsync(CreateCompanyDTO companyDTO, string ownerEmail);
        public Task<bool> DeleteCompanyAsync(int companyId);
        public Task AddEmployeesToCompany(int companyId, List<string> UserEmails);
        public Task<bool> RemoveEmployeeFromCompany(int companyId, string workerId);
        public Task<GetCompanyDTO> GetCompany(int companyId);

    }
}
