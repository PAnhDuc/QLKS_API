using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS_API.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; } = null!;
        public List<Booking> Bookings { get; set; } = new();
        public List<Invoice> Invoices { get; set; } = new();
    }
}