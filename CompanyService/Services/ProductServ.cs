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
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IBookingValidationService bookingValidator;


        public ProductServ(Context context, IPublishEndpoint publishEndpoint, IBookingValidationService bookingValidator)
        {
            dbcontext = context;

            _publishEndpoint = publishEndpoint;
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

        public async Task DeleteProductAsync(int id)
        {
            var product = await dbcontext.Products.FindAsync(id) ?? throw new NotFoundException("Product not found " + id);

            if (await bookingValidator.HasActiveBookingsProduct(id))
            {
                throw new BadRequestException("Cannot remove product with active bookings. Id: " + id);
            }

            dbcontext.Products.Remove(product);
            await dbcontext.SaveChangesAsync();


            /* await _publishEndpoint.Publish(new ProductForCompanyDeleted
             {
                 ProductId = product.Id
             });*/
        }
    }
}
