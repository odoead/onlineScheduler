using CompanyService.DTO;
using CompanyService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CompanyService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private IProductService productService;
        public ProductController(IProductService productService)
        {
            this.productService = productService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddProduct([FromBody] CreateProductDTO ProductDTO)
        {
            var productId = await productService.AddProductAsync(
                ProductDTO.Name,
                ProductDTO.Description,
                ProductDTO.Duration,
                ProductDTO.CompanyId,
                ProductDTO.WorkerIds
            );

            return Ok(productId);

        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await productService.GetProductAsync(id);
            return Ok(product);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDTO updateProductDTO)
        {
            await productService.UpdateProductAsync(
                id,
                updateProductDTO.Name,
                updateProductDTO.Description,
                updateProductDTO.Duration,
                updateProductDTO.WorkerIds
            );
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await productService.DeleteProductAsync(id);
            return NoContent();
        }
        [HttpPost("{productId}/workers/{workerId}")]
        [Authorize]
        public async Task<IActionResult> AssignWorkerToService(int productId, string workerId)
        {
            var result = await productService.AssignWorkerToServiceAsync(productId, workerId);
            return Ok(result);
        }

        [HttpDelete("{productId}/workers/{workerId}")]
        [Authorize]
        public async Task<IActionResult> RemoveWorkerFromService(int productId, string workerId)
        {
            var result = await productService.RemoveWorkerFromServiceAsync(productId, workerId);
            return Ok(result);
        }

        [HttpGet("company/{companyId}")]
        [Authorize]
        public async Task<IActionResult> GetAllProductsByCompany(int companyId)
        {
            var products = await productService.GetAllProductsByCompanyAsync(companyId);
            return Ok(products);
        }
    }
}
