namespace ProductService.DTO
{
    public class GetProductAndWorkersDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public List<WorkerDTO> Workers { get; set; }
        public int CompanyId { get; set; }

    }
}
