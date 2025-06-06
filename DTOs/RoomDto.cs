using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QLKS_API.Models;

namespace QLKS_API.DTOs
{
    public class RoomDto
    {
        public string RoomNumber { get; set; } = string.Empty;
        public string? RoomType { get; set; }
        public decimal Price { get; set; }
        public RoomStatus Status { get; set; } = RoomStatus.Available;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; } // Thêm trường này
    }
}