﻿namespace CompanyService.DTO
{
    public class CreateProductDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public int CompanyId { get; set; }
        public List<string> WorkerIds { get; set; }
    }
}
