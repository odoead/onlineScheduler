namespace Shared.Events.Product
{
    public class ProductForCompanyEdited
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public List<string> WorkerIds { get; set; }

    }
}
