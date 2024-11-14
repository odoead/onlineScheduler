namespace ProductService.Entities
{
    public class Worker
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<ProductWorker> Products { get; set; }
    }
}
