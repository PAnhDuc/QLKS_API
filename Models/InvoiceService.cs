using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS_API.Models
{
    public class InvoiceService
    {
        [Key]
        [Column("invoice_service_id")]
        public int InvoiceId { get; set; }
        [Column("service_id")]
        public int ServiceId { get; set; }
        [Column("quantity")]
        public int Quantity { get; set; } = 1;
        public Invoice Invoice { get; set; } = null!;
        public Service Service { get; set; } = null!;
    }
}