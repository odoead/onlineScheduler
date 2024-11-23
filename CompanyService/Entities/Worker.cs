using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyService.Entities
{
    public class Worker
    {
        [Key]
        public string Id { get; set; }
        public string IdentityServiceId { get; set; }
        public string FullName { get; set; }
        public Company OwnedCompany { get; set; }

        public List<ScheduleInterval> ScheduleIntervals { get; set; }
        public List<CompanyWorker> CompanyWorkAssignments { get; set; }
        public List<ProductWorker> AssignedProducts { get; set; }
        public List<Booking> Bookings { get; set; }
    }


    public class CompanyWorker
    {
        [ForeignKey("CompanyID")]
        public SharedCompany Company { get; set; }
        public int CompanyID { get; set; }

        [ForeignKey("WorkerId")]
        public Worker Worker { get; set; }
        public string WorkerId { get; set; }
    }

    public class ProductWorker
    {
        [ForeignKey("WorkerId")]
        public Worker Worker { get; set; }
        public string WorkerId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        public int ProductId { get; set; }
    }
}
