using CompanyService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private readonly IProoductService _ProductService;

        public ProductController(IProoductService ProductService)
        {
            _ProductService = ProductService;
        }


    }
}
