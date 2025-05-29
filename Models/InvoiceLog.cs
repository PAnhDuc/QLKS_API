using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKS_API.Models
{
    [Table("invoice_log")]
    public class InvoiceLog
    {
        [Key]
        [Column("log_id")]
        public int LogId { get; set; }
        [Column("status")]
        public string Status { get; set; } = string.Empty;
        [Column("changed_at")]
        public DateTime ChangedAt { get; set; }
        [Column("invoice_id")]
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;
        [Column("created_by")]
        public int CreatedBy { get; set; } // hoặc string nếu lưu username
    }
}