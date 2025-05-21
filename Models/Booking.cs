using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS_API.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; }
        public Customer Customer { get; set; } = null!;
        public Room Room { get; set; } = null!;
        public Invoice? Invoice { get; set; }
    }
}