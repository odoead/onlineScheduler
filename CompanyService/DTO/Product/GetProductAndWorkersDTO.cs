using CompanyService.DTO.Company;
using CompanyService.DTO.Worker;

namespace CompanyService.DTO
{
    public class GetProductAndWorkersDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public List<WorkerMinDTO> Workers { get; set; }
        public CompanyMinDTO Company { get; set; }

    }
}
