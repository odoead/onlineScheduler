namespace CompanyService.DTO.Worker
{
    public class WorkerEmptySchedulesDTO
    {
        public string WorkerId { get; set; }
        public string WorkerName { get; set; }
        public List<ScheduleEmptyWindow> EmptySchedules { get; set; }
    }
}
