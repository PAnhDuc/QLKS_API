using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS_API.Models
{
    public class Report
    {
        public int ReportId { get; set; }
        public int UserId { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public string ReportData { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public User User { get; set; } = null!;
    }
}