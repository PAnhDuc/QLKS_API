using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QLKS_API.Models;

namespace QLKS_API.DTOs
{
    public class InvoiceDto
    {
        public int InvoiceId { get; set; }
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public int Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}