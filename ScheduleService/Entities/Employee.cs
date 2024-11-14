using System.ComponentModel.DataAnnotations;

namespace ScheduleService.Entities
{
    public class Employee
    {
        [Key]
        public string Id { get; set; }
        public string FullName { get; set; }
    }
}
