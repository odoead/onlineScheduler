using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyService.Entities
{
    public class CompanySettings
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
        public string CompanyId { get; set; }

        public bool DoesSendWorkerNotificationOnBookingCreated { get; set; }
        public int? TimeBeforeBookingStartWhenNotScheduleNotifyClientInMinutes_OnBookingCreated { get; set; }

        public bool DoesSendClientNotificationOnBookingConfirmed { get; set; }


        public bool DoesSendClientNotificationOnBookingEdited { get; set; }

        public bool DoesSendWorkerNotificationOnBookingCanceled { get; set; }
        public bool DoesSendClientNotificationOnBookingCanceled { get; set; }

        public bool DoesScheduleNotifyClientOnIncomingBooking { get; set; }
        public bool DoesScheduleNotifyWorkerOnIncomingBooking { get; set; }
        public int? TimeBeforeBookingStartWhenScheduleNotifyInMinutes_OnBookingIncoming { get; set; }
        public int? TimeBeforeBookingStartWhenScheduleNotifyInMinutes_OnBookingIncoming_ClientLong { get; set; }

    }
}
