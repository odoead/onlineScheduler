using Shared.Data;

namespace Shared.Messages.Company
{
    public class CompanyCreated
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string OwnerId { get; set; }
        public TimeSpan OpeningTimeLOC { get; set; }
        public TimeSpan ClosingTimeLOC { get; set; }
        public int CompanyType { get; set; }
        public List<string> EmployeeIds { get; set; }
        public List<DayOfTheWeek> WorkingDays;

    }
}
