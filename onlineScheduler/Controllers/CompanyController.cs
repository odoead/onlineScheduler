using CompanyService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace onlineScheduler.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        /*[HttpPost]
        public async Task<IActionResult> AddCompany([FromBody] CreateCompanyDTO company)
        {
            var companyId = await _companyService.AddCompanyAsync(company);
            return Ok(companyId);
        }*/

        [HttpDelete("{companyId:int}")]
        public async Task<IActionResult> DeleteCompany(int companyId)
        {
            var result = await _companyService.DeleteCompanyAsync(companyId);
            if (!result) return NotFound();
            return Ok();
        }

        /*[HttpPut("updateemployees/{companyId:int}")]
        public async Task<IActionResult> UpdateCompanyEmployees(int companyId, [FromBody] List<string> employeeIds)
        {
            var result = await _companyService.UpdateCompanyEmployeesAsync(companyId, employeeIds);
            if (!result) return NotFound();
            return Ok();
        }*/
    }
}
