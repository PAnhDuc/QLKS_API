using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace QLKS_API.Models
{
    public class InvoiceLog
    {
        [Key]
        public int LogId { get; set; }
        public int InvoiceId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public Invoice Invoice { get; set; } = null!;
    }
}