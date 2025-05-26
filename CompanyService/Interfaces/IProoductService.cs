using CompanyService.DTO;

namespace CompanyService.Interfaces
{
    public interface IProductService
    {
        public Task<int> AddProductAsync(string Name, string Description, TimeSpan Duration, int CompanyId, List<string> WorkerIds);
        public Task<GetProductAndWorkersDTO> GetProductAsync(int id);
        public Task UpdateProductAsync(int id, string Name, string Description, TimeSpan Duration, List<string> WorkerIds);
        public Task<bool> DeleteProductAsync(int id);
        public Task<bool> AssignWorkerToServiceAsync(int productId, string workerId);
        public Task<bool> RemoveWorkerFromServiceAsync(int productId, string workerId);
        public Task<List<GetProductAndWorkersDTO>> GetAllProductsByCompanyAsync(int companyId);
        ///TODO

    }
}
