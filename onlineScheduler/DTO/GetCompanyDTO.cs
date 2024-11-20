using Shared.Data;

namespace CompanyService.DTO
{
    public class GetCompanyDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public TimeSpan OpeningTimeLOC { get; set; }
        public TimeSpan ClosingTimeLOC { get; set; }
        public CompanyType CompanyType { get; set; }
        public string OwnerId { get; set; }
        public string OwnerName { get; set; }
        public List<ProductDTO> Products { get; set; }
        public List<WorkerMinDTO> Workers { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
