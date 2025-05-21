using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS_API.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; }
        public Booking Booking { get; set; } = null!;
        public Customer Customer { get; set; } = null!;
        public List<InvoiceService> InvoiceServices { get; set; } = new();
        public List<InvoiceLog> InvoiceLogs { get; set; } = new();
    }
}