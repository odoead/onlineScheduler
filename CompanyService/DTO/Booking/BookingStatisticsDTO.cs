namespace CompanyService.DTO.Booking
{
    public class BookingStatisticsDTO
    {
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int PendingBookings { get; set; }
        public List<BookingProductStatDTO> BookingsByProduct { get; set; }
        public List<BookingWorkerStatDTO> BookingsByWorker { get; set; }
        public List<BookingDayStatDTO> BookingsByDay { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class BookingProductStatDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int BookingCount { get; set; }
    }

    public class BookingWorkerStatDTO
    {
        public string WorkerId { get; set; }
        public string WorkerName { get; set; }
        public int BookingCount { get; set; }
    }

    public class BookingDayStatDTO
    {
        public DateTime Date { get; set; }
        public int BookingCount { get; set; }
    }
}
