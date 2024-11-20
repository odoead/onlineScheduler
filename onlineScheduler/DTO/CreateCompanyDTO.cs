namespace CompanyService.DTO
{
    public class CreateCompanyDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public TimeSpan OpeningTimeLOC { get; set; }
        public TimeSpan ClosingTimeLOC { get; set; }
        public int CompanyType { get; set; }
        public List<int> WorkingDays { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
