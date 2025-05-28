using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS_API.Models
{
    public class Service
    {
        [Key]
        [Column("service_id")]
        public int ServiceId { get; set; }
        [Column("service_name")]
        public string ServiceName { get; set; } = string.Empty;
        [Column("price")]
        public decimal Price { get; set; }
        [Column("description")]
        public string? Description { get; set; }
        public List<InvoiceService> InvoiceServices { get; set; } = new();
    }
}