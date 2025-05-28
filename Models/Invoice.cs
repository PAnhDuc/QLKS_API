using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS_API.Models
{
    public class Invoice
    {
        [Key]
        [Column("invoice_id")]
        public int InvoiceId { get; set; }
        [Column("booking_id")]
        public int BookingId { get; set; }
        [Column("customer_id")]
        public int CustomerId { get; set; }
        [Column("total_amount")]
        public decimal TotalAmount { get; set; }
        [Column("status")]
        public string Status { get; set; } = "pending";
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        public Booking Booking { get; set; } = null!;
        public Customer Customer { get; set; } = null!;
        public List<InvoiceService> InvoiceServices { get; set; } = new();
        public List<InvoiceLog> InvoiceLogs { get; set; } = new();
    }
}