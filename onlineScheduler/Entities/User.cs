using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyService.Entities
{
    public class User
    {
        [Key]
        public string Id { get; set; }
        public string FullName { get; set; }
        public UserType UserType { get; set; }

        public Company Company { get; set; }

        public List<CompanyWorkers> CompanyWorkers { get; set; }
    }
    public enum UserType
    {
        Worker,
        Owner
    }

    [Keyless]
    public class CompanyWorkers
    {
        [ForeignKey("CompanyID")]
        public SharedCompany Company { get; set; }
        public int CompanyID { get; set; }

        [ForeignKey("WorkerId")]
        public User Worker { get; set; }
        public string WorkerId { get; set; }

    }
}
