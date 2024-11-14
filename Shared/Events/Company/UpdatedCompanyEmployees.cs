using Shared.Data;

namespace Shared.Messages.Company
{
    public class UpdatedCompanyEmployees
    {
        public int CompanyId { get; set; }
        public List<string> EmployeeIds { get; set; }
        public TimeSpan OpeningTimeLOC { get; set; }
        public TimeSpan ClosingTimeLOC { get; set; }
        public List<DayOfTheWeek> WorkingDays;

    }
}
