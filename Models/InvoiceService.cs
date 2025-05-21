using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS_API.Models
{
    public class InvoiceService
    {
        public int InvoiceId { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; } = 1;
        public Invoice Invoice { get; set; } = null!;
        public Service Service { get; set; } = null!;
    }
}