using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events.Booking
{
    public class BookingCreated
    {
        public int BookingId { get; set; }
        public DateTime StartDateLOC { get; set; }
        public DateTime EndDateLOC { get; set; }
        public string ClientId { get; set; }
        public string WorkerId { get; set; }
        public int ProductId { get; set; }
        public int CompanyId { get; set; }
            
    }
}
