using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QLKS_API.Models;

namespace QLKS_API.DTOs
{
    public class UpdateBookingDto
    {
        public int CustomerId { get; set; }
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public BookingStatus Status { get; set; }
    }
}