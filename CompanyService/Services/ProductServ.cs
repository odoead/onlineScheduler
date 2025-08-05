using CompanyService.DB;
using CompanyService.DTO;
using CompanyService.DTO.Company;
using CompanyService.DTO.Worker;
using CompanyService.Entities;
using CompanyService.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions.custom_exceptions;

namespace CompanyService.Services
{
    public class ProductServ : IProductService
    {
        private readonly Context dbcontext;
        private readonly IPublishEndpoint publishEndpoint;
        private readonly IBookingValidationService bookingValidator;

        public ProductServ(Context context, IPublishEndpoint publishEndpoint, IBookingValidationService bookingValidator)
        {
            dbcontext = context;

            this.publishEndpoint = publishEndpoint;
            this.bookingValidator = bookingValidator;
        }
        public async Task<int> AddProductAsync(string Name, string Description, TimeSpan Duration, int CompanyId, List<string> WorkerIds)
        {
            var product = new Product
            {
                Name = Name,
                Description = Description,
                Duration = Duration,
                CompanyId = CompanyId,
                AssignedWorkers = WorkerIds.Select(wId => new ProductWorker { WorkerId = wId }).ToList(),
            };

            dbcontext.Products.Add(product);
            await dbcontext.SaveChangesAsync();

            /*  await _publishEndpoint.Publish(new ProductForCompanyCreated
              {
                  WorkerIds = productDto.WorkerIds,
                  ProductID = productDto.CompanyId,
                  Name = productDto.Name,
                  CompanyId = productDto.CompanyId,
                  DurationTime = productDto.Duration,
                  Description = productDto.Description,
              });*/
            return product.Id;
        }

        public async Task<GetProductAndWorkersDTO> GetProductAsync(int id)
        {
            var product = await dbcontext.Products.Include(q => q.Company).Include(q => q.AssignedWorkers).ThenInclude(q => q.Worker)
                .Where(p => p.Id == id)
                .Select(p => new GetProductAndWorkersDTO
                {
                    Name = p.Name,
                    Description = p.Description,
                    Duration = p.Duration,
                    Company = new CompanyMinDTO { Id = p.CompanyId, Name = p.Company.Name },
                    Workers = p.AssignedWorkers.Select(q => new WorkerMinDTO { Id = q.WorkerId, Name = q.Worker.FullName, }).ToList(),
                })
                .FirstOrDefaultAsync();

            return product;
        }

        public async Task UpdateProductAsync(int id, string Name, string Description, TimeSpan Duration, List<string> WorkerIds)
        {
            var product = await dbcontext.Products.FindAsync(id) ?? throw new NotFoundException("Product not found with that id " + id);

            if (await bookingValidator.HasActiveBookingsProduct(id))
            {
                throw new BadRequestException("Cannot remove product with active bookings. Id: " + id);
            }

            product.Name = Name;
            product.Description = Description;
            product.Duration = Duration;
            product.AssignedWorkers = WorkerIds.Select(wId => new ProductWorker { WorkerId = wId }).ToList();

            await dbcontext.SaveChangesAsync();

            /*await _publishEndpoint.Publish(new ProductForCompanyEdited
            {
                Description = productDto.Description,
                Duration = productDto.Duration,
                Name = productDto.Name,
                ProductID = Id,
                WorkerIds = productDto.WorkerIds,
            });*/
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await dbcontext.Products.FindAsync(id) ?? throw new NotFoundException("Product not found " + id);

            if (await bookingValidator.HasActiveBookingsProduct(id))
            {
                throw new BadRequestException("Cannot remove product with active bookings. Id: " + id);
            }

            dbcontext.Products.Remove(product);
            await dbcontext.SaveChangesAsync();
            return true;

            /* await _publishEndpoint.Publish(new ProductForCompanyDeleted
             {
                 ProductId = product.Id
             });*/
        }

        public async Task<bool> AssignWorkerToServiceAsync(int productId, string workerId)
        {
            var product = await dbcontext.Products.Include(p => p.AssignedWorkers).FirstOrDefaultAsync(p => p.Id == productId) ??
                throw new NotFoundException("Product not found with id " + productId);

            var worker = await dbcontext.Workers.FirstOrDefaultAsync(w => w.Id == workerId) ??
                throw new NotFoundException("Worker not found with id " + workerId);

            if (product.AssignedWorkers.Any(pw => pw.WorkerId == workerId))
            {
                return false;
            }

            product.AssignedWorkers.Add(new ProductWorker
            {
                ProductId = productId,
                WorkerId = workerId
            });

            await dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveWorkerFromServiceAsync(int productId, string workerId)
        {
            var productWorker = await dbcontext.ProductWorkers.FirstOrDefaultAsync(pw => pw.ProductId == productId && pw.WorkerId == workerId);
            if (productWorker == null)
                return false;

            var hasAnyActiveBookings = await dbcontext.Bookings.AnyAsync(b => b.ProductId == productId && b.WorkerId == workerId && b.EndDateLOC > DateTime.UtcNow);

            if (hasAnyActiveBookings)
                throw new BadRequestException("Cannot remove worker from product with active bookings");

            dbcontext.ProductWorkers.Remove(productWorker);
            await dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<List<GetProductAndWorkersDTO>> GetAllProductsByCompanyAsync(int companyId)
        {
            var company = await dbcontext.Companies.FirstOrDefaultAsync(c => c.Id == companyId) ??
                throw new NotFoundException("Company not found with id " + companyId);

            var products = await dbcontext.Products.Include(p => p.Company).Include(p => p.AssignedWorkers).ThenInclude(pw => pw.Worker)
                .Where(p => p.CompanyId == companyId).Select(p => new GetProductAndWorkersDTO
                {
                    Name = p.Name,
                    Description = p.Description,
                    Duration = p.Duration,
                    Company = new CompanyMinDTO { Id = p.CompanyId, Name = p.Company.Name },
                    Workers = p.AssignedWorkers.Select(pw => new WorkerMinDTO
                    {
                        Id = pw.WorkerId,
                        Name = pw.Worker.FullName,
                    }).ToList(),
                }).ToListAsync();

            return products;
        }
    }
}
