﻿namespace Shared.Events.Booking
{
    public class BookingCreated
    {
        public int BookingId { get; set; }
        public DateTime StartDateLOC { get; set; }
        public DateTime? EndDateLOC { get; set; }
        public string ClientId { get; set; }
        public string WorkerId { get; set; }
        public int ProductId { get; set; }

    }
}
