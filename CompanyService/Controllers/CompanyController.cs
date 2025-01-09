using CompanyService.DTO.Company;
using CompanyService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CompanyService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService companyService;

        public CompanyController(ICompanyService companyService)
        {
            this.companyService = companyService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddCompany([FromBody] CreateCompanyDTO createCompanyDTO)
        {
            var emailClaim = User?.FindFirst(ClaimTypes.Email)?.Value;
            if (emailClaim == null)
            {
                return Unauthorized();
            }

            var companyId = await companyService.AddCompanyAsync(
                    createCompanyDTO.Name,
                    createCompanyDTO.Description,
                    createCompanyDTO.OpeningTimeLOC,
                    createCompanyDTO.ClosingTimeLOC,
                    createCompanyDTO.CompanyType,
                    createCompanyDTO.WorkingDays,
                    createCompanyDTO.Latitude,
                    createCompanyDTO.Longitude,
                    emailClaim);
            return Ok(companyId);
        }

        [HttpDelete("{companyId}")]
        [Authorize]
        public async Task<IActionResult> DeleteCompany(int companyId)
        {
            var result = await companyService.DeleteCompanyAsync(companyId);
            return Ok(result);
        }

        [HttpPost("{companyId}/employees")]
        [Authorize]
        public async Task<IActionResult> AddEmployeesToCompany(int companyId, [FromBody] List<string> userEmails)
        {
            await companyService.AddEmployeesToCompany(companyId, userEmails);
            return Ok();
        }

        [HttpDelete("{companyId}/employees/{workerId}")]
        [Authorize]
        public async Task<IActionResult> RemoveEmployeeFromCompany(int companyId, string workerId)
        {
            var result = await companyService.RemoveEmployeeFromCompany(companyId, workerId);
            return NoContent();
        }

        [HttpGet("{companyId}")]
        [Authorize]
        public async Task<IActionResult> GetCompany(int companyId)
        {
            var company = await companyService.GetCompany(companyId);
            return Ok(company);
        }

        [HttpGet()]
        public async Task<IActionResult> GetCompanies()
        {

            var cms = await companyService.GetCompaniesMin();
            return Ok(cms);
        }
    }
}
