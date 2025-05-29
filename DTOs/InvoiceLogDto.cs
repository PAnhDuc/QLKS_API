using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS_API.DTOs
{
    public class InvoiceLogDto
    {
        public int LogId { get; set; }
        public int BookingId { get; set; }
        public string Status { get; set; }
        public DateTime ChangedAt { get; set; }
        public int CreatedBy { get; set; }
    }
}