using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKS_API.Models
{
    public class Report
    {
        [Key]
        [Column("report_id")]
        public int ReportId { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
        [Column("report_type")]
        public string ReportType { get; set; } = string.Empty;
        [Column("report_data")]
        public string ReportData { get; set; } = string.Empty;
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        public User User { get; set; } = null!;
    }
}