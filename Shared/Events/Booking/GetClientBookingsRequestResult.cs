namespace Shared.Events.Booking
{
    public class GetClientBookingsRequestResult
    {

        public List<ClientBooking_ScheduleService> Bookings { get; set; }

    }
    public class ClientBooking_ScheduleService
    {
        public int BookingId { get; set; }
        public int ProductId { get; set; }
        public DateTime StartDateUTC { get; set; }
        public DateTime EndDateUTC { get; set; }

        public string Status { get; set; }

    }
}
