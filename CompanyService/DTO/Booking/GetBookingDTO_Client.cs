using CompanyService.DTO.Product;

namespace CompanyService.DTO.Booking
{
    public class GetBookingDTO_Client
    {
        public int Id { get; set; }
        public ProductMinDTO Product { get; set; }
        public DateTime StartDateLOC { get; set; }
        public DateTime EndDateLOC { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Status { get; set; }
        public string CompanyName { get; set; }
    }
}
