using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKS_API.Models
{
    public class Customer
    {
        [Key]
        [Column("customer_id")]
        public int CustomerId { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
        [Column("full_name")]
        public string FullName { get; set; } = string.Empty;
        [Column("email")]
        public string? Email { get; set; }
        [Column("phone")]
        public string? Phone { get; set; }
        [Column("address")]
        public string? Address { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        public User User { get; set; } = null!;
        [InverseProperty("Customer")]
        public List<Booking> Bookings { get; set; } = new();
        public List<Invoice> Invoices { get; set; } = new();
    }
}