using CompanyService.DTO;

namespace CompanyService.Interfaces
{
    public interface ICompanyService
    {
        public Task<int> AddCompanyAsync(CreateCompanyDTO company);
        public Task<bool> DeleteCompanyAsync(int companyId);
        public Task<bool> UpdateCompanyEmployeesAsync(int companyId, List<string> employeeIds);
    }
}
