using CompanyService.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProductService.DB;
using ProductService.DTO;
using ProductService.Entities;
using Shared.Events.Product;
using Shared.Exceptions.custom_exceptions;

namespace ProductService.Services
{
    public class ProductServ : IProoductService
    {
        private readonly Context dbcontext;
        private readonly IPublishEndpoint _publishEndpoint;


        public ProductServ(Context context, IPublishEndpoint publishEndpoint)
        {
            dbcontext = context;

            _publishEndpoint = publishEndpoint;
        }
        public async Task<int> AddProductAsync(CreateProductDTO productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Duration = productDto.Duration,
                CompanyId = productDto.CompanyId,
                Workers = productDto.WorkerIds.Select(wId => new ProductWorker { WorkerId = wId }).ToList(),
            };

            dbcontext.Products.Add(product);
            await dbcontext.SaveChangesAsync();

            await _publishEndpoint.Publish(new ProductForCompanyCreated
            {
                WorkerIds = productDto.WorkerIds,
                ProductID = productDto.CompanyId,
                Name = productDto.Name,
                CompanyId = productDto.CompanyId,
                DurationTime = productDto.Duration,
                Description = productDto.Description,
            });
            return product.Id;
        }

        // Get Product
        public async Task<GetProductDTO> GetProductAsync(int id)
        {
            var product = await dbcontext.Products
                .Where(p => p.Id == id)
                .Select(p => new GetProductDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Duration = p.Duration,
                    CompanyId = p.CompanyId,
                    WorkerIds = p.Workers.Select(q => q.WorkerId).ToList(),
                })
                .FirstOrDefaultAsync();



            return product;
        }

        public async Task<GetProductAndWorkersDTO> GetProductWorkersAsync(int id)
        {
            var product = await dbcontext.Products.Include(q => q.Workers).ThenInclude(q => q.Worker)
                .Where(p => p.Id == id)
                .Select(p => new GetProductAndWorkersDTO
                {
                    Name = p.Name,
                    Description = p.Description,
                    Duration = p.Duration,
                    CompanyId = p.CompanyId,
                    Workers = p.Workers.Select(q => new WorkerDTO { Id = q.WorkerId, Name = q.Worker.Name }).ToList(),
                })
                .FirstOrDefaultAsync();



            return product;
        }



        // Update Product
        public async Task UpdateProductAsync(int Id, UpdateProductDTO productDto)
        {
            var product = await dbcontext.Products.FindAsync(Id);
            if (product == null)
                throw new NotFoundException("Product not found with that id " + Id);

            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.Duration = productDto.Duration;
            product.Workers = productDto.WorkerIds.Select(wId => new ProductWorker { WorkerId = wId }).ToList();

            await dbcontext.SaveChangesAsync();


            await _publishEndpoint.Publish(new ProductForCompanyEdited
            {
                Description = productDto.Description,
                Duration = productDto.Duration,
                Name = productDto.Name,
                ProductID = Id,
                WorkerIds = productDto.WorkerIds,
            });
        }

        // Delete Product
        public async Task DeleteProductAsync(int id)
        {
            var product = await dbcontext.Products.FindAsync(id);
            if (product == null)
                throw new NotFoundException("Product not found " + id);

            dbcontext.Products.Remove(product);
            await dbcontext.SaveChangesAsync();


            await _publishEndpoint.Publish(new ProductForCompanyDeleted
            {
                ProductId = product.Id
            });
        }
    }
}
