using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKS_API.Models
{
    public class InvoiceLog
    {
        [Key]
        [Column("log_id")]
        public int LogId { get; set; }
        [Column("booking_id")]
        public int BookingId { get; set; }
        [Column("status")]
        public string Status { get; set; } = string.Empty;
        [Column("changed_at")]
        public DateTime ChangedAt { get; set; }
        public Invoice Invoice { get; set; } = null!;
    }
}