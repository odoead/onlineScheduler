using CompanyService.DTO.Product;
using CompanyService.DTO.Worker;
using Shared.Data;

namespace CompanyService.DTO.Company
{
    public class GetCompanyDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TimeSpan OpeningTimeLOC { get; set; }
        public TimeSpan ClosingTimeLOC { get; set; }
        public CompanyType CompanyType { get; set; }
        public string OwnerId { get; set; }
        public string OwnerName { get; set; }
        public List<ProductMinDTO> Products { get; set; }
        public List<WorkerMinDTO> Workers { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
