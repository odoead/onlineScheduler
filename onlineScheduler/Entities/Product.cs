using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyService.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public int DurationTime { get; set; }
        public string Title { get; set; }


        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
        public int CompanyId { get; set; }
    }
}
