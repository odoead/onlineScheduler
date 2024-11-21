﻿using CompanyService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTO;

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
    }
}
