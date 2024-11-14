using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProductService.DB;
using ProductService.Entities;
using Shared.Exceptions.custom_exceptions;
using Shared.Messages.Company;

namespace ProductService.Consumers
{
    public class UpdateCompanyEmployeesConsumer : IConsumer<UpdatedCompanyEmployees>
    {
        private readonly Context dbcontext;

        public UpdateCompanyEmployeesConsumer(Context context)
        {
            dbcontext = context;
        }
        public async Task Consume(ConsumeContext<UpdatedCompanyEmployees> context)
        {
            var message = context.Message;
            var products = await dbcontext.Products
           .Where(p => p.CompanyId == message.CompanyId)
           .ToListAsync();

            if (!products.Any())
            {
                throw new NotFoundException("products for this company not found " + message.CompanyId);
            }
            var workers = await dbcontext.Workers
                .Where(w => message.EmployeeIds.Contains(w.Id))
                .ToListAsync();

            if (workers.Count != message.EmployeeIds.Count)
            {
                throw new NotFoundException("workers for this company not found " + message.CompanyId);
            }

            foreach (var product in products)
            {
                var existWorkersRels = await dbcontext.ProductWorkers
                    .Where(q => q.ProductId == product.Id)
                    .ToListAsync();

                dbcontext.ProductWorkers.RemoveRange(existWorkersRels);

                var newProductWorkerRels = workers.Select(worker => new ProductWorker
                {
                    ProductId = product.Id,
                    WorkerId = worker.Id
                }).ToList();

                await dbcontext.ProductWorkers.AddRangeAsync(newProductWorkerRels);
            }
            // Сохраняем изменения в БД
            await dbcontext.SaveChangesAsync();
        }
    }
}
