﻿using Shared.Data;
using System.ComponentModel.DataAnnotations;

namespace BookingService.Entities
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }
        public string WorkerId { get; set; }
        public string ClientId { get; set; }
        public int ProductId { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.CREATED;
        public DateTime StartDateUTC { get; set; }
        public DateTime? EndDateUTC { get; set; }
        public ServiceType Service { get; set; }

    }
    public enum ServiceType//например возможность иметь обработку нескольких сервисов 
    {
        SCHEDULE
        //, delivery
    }
}
