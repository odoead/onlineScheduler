using CompanyService.DB;
using CompanyService.DTO.Company;
using CompanyService.DTO.Worker;
using CompanyService.Entities;
using CompanyService.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProductService.DTO;
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
        public async Task<int> AddProductAsync(CreateProductDTO productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Duration = productDto.Duration,
                CompanyId = productDto.CompanyId,
                AssignedWorkers = productDto.WorkerIds.Select(wId => new ProductWorker { WorkerId = wId }).ToList(),
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
                    Workers = p.AssignedWorkers.Select(q => new WorkerMinDTO { Id = q.WorkerId, Name = q.Worker.FullName }).ToList(),
                })
                .FirstOrDefaultAsync();

            return product;
        }





        // Update Product
        public async Task UpdateProductAsync(int id, UpdateProductDTO productDto)
        {
            var product = await dbcontext.Products.FindAsync(id) ?? throw new NotFoundException("Product not found with that id " + id);

            if (await bookingValidator.HasActiveBookingsProduct(id))
            {
                throw new BadRequestException("Cannot remove product with active bookings. Id: " + id);
            }

            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.Duration = productDto.Duration;
            product.AssignedWorkers = productDto.WorkerIds.Select(wId => new ProductWorker { WorkerId = wId }).ToList();

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

        // Delete Product
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
