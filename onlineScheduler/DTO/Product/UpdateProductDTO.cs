namespace ProductService.DTO
{
    public class UpdateProductDTO
    {

        public string Name { get; set; }
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public List<string> WorkerIds { get; set; }
    }
}
