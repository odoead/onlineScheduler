namespace Shared.Events.Product
{
    public class ProductForCompanyCreated
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public TimeSpan DurationTime { get; set; }
        public string Description { get; set; }
        public List<string> WorkerIds { get; set; }

    }
}
