using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS_API.DTOs
{
    public class RevenueReportDto
    {
        public int UserId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}