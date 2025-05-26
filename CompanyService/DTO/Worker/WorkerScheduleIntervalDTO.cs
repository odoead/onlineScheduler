namespace CompanyService.DTO.Worker
{
    public class WorkerScheduleIntervalDTO
    {
        public string WorkerId { get; set; }
        public string WorkerName { get; set; }
        public List<ScheduleIntervalDTO> EmptySchedules { get; set; }
    }
}
