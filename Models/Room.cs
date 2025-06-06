using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS_API.Models
{
    [Table("Rooms")]
    public class Room
    {
        [Key]
        [Column("room_id")]
        public int RoomId { get; set; }

        [Column("room_number")]
        public string RoomNumber { get; set; } = string.Empty;

        [Column("room_type")]
        public string? RoomType { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("status")]
        public RoomStatus Status { get; set; } = RoomStatus.Available;

        [Column("description")]
        public string? Description { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; } // Added this field

        public List<Booking> Bookings { get; set; } = new();
    }
}