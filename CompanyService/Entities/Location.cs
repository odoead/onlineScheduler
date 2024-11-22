using NpgsqlTypes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyService.Entities
{
    public class Location
    {
        [Key]
        public int Id { get; set; }
        public NpgsqlPoint Coordinates { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
        [Required]
        public int CompanyId { get; set; }
    }
}
