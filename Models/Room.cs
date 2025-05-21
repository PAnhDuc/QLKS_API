using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS_API.Models
{
    public class Room
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string? RoomType { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = "available";
        public string? Description { get; set; }
        public List<Booking> Bookings { get; set; } = new();
    }
}