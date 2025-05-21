using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS_API.DTOs
{
    public class InvoiceServiceDto
    {
        public int InvoiceId { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}