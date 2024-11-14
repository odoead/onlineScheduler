using Shared.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyService.Entities
{
    public abstract class Company
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TimeSpan OpeningTimeLOC { get; set; }
        public TimeSpan ClosingTimeLOC { get; set; }
        [ForeignKey("OwnerId")]
        public User Owner { get; set; }
        public string OwnerId { get; set; }
        public List<Product> Products { get; set; }
        public Location Location { get; set; }
        public TimeSpan TimeZoneFromUTCOffset { get; set; }
        [NotMapped]
        public CompanyType CompanyType { get; set; }
        [Column("WorkingDaysSerialized")]
        public string NotUseWorkingDaysSerialized { get; set; }
        [NotMapped]
        public List<DayOfTheWeek> WorkingDays
        {
            get
            {
                return string.IsNullOrEmpty(NotUseWorkingDaysSerialized) ?
                    new List<DayOfTheWeek>() : NotUseWorkingDaysSerialized.Split(',').Select(int.Parse).Cast<DayOfTheWeek>().ToList();
            }
            set
            {
                NotUseWorkingDaysSerialized = value != null ? string.Join(",", value.Select(v => (int)v)) : string.Empty;
            }
        }
    }

    public class SharedCompany : Company
    {


        public List<CompanyWorkers> Workers { get; set; }
    }

    public class PersonalCompany : Company
    {

        [ForeignKey("WorkerId")]
        public User Worker { get; set; }
        public string WorkerId { get; set; }
    }


}
