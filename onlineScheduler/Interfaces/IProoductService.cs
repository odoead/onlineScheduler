using ProductService.DTO;

namespace CompanyService.Interfaces
{
    public interface IProductService
    {
        public Task<int> AddProductAsync(string Name, string Description, TimeSpan Duration, int CompanyId, List<string> WorkerIds);
        public Task<GetProductAndWorkersDTO> GetProductAsync(int id);
        public Task UpdateProductAsync(int id, string Name, string Description, TimeSpan Duration, List<string> WorkerIds);
        public Task DeleteProductAsync(int id);


    }
}
