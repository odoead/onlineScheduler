using CompanyService.DTO.Product;

namespace CompanyService.DTO.Booking
{
    public class GetBookingDTO_Worker
    {
        public int Id { get; set; }
        public ProductMinDTO Product { get; set; }
        public DateTime StartDateLOC { get; set; }
        public DateTime EndDateLOC { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerName { get; set; }
        public string Status { get; set; }
        public string CompanyName { get; set; }
    }
}
