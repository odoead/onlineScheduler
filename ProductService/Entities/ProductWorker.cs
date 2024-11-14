using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Entities
{
    [Keyless]
    public class ProductWorker
    {

        [ForeignKey("ProductID")]
        public Product Product { get; set; }
        public int ProductId { get; set; }

        [ForeignKey("WorkerId")]
        public Worker Worker { get; set; }
        public string WorkerId { get; set; }
    }
}
