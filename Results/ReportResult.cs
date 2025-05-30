using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS_API.Results
{
    public class ReportResult
    {
        [Column("report_id")]
        public decimal ReportId { get; set; }
        [Column("message")]
        public string Message { get; set; }
    }
}