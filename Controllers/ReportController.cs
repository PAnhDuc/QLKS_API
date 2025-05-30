using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKS_API.Data;
using QLKS_API.DTOs;
using QLKS_API.Results;
using Microsoft.Data.SqlClient; // Sử dụng Microsoft.Data.SqlClient

namespace QLKS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Accountant")]
    public class ReportController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public ReportController(HotelDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetReports()
        {
            var reports = await _context.Reports.Include(r => r.User).ToListAsync();
            return Ok(reports);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReport(int id)
        {
            var report = await _context.Reports.Include(r => r.User).FirstOrDefaultAsync(r => r.ReportId == id);
            if (report == null) return NotFound();
            return Ok(report);
        }

        [HttpPost("monthly-revenue")]
        public async Task<IActionResult> GenerateMonthlyRevenueReport([FromBody] RevenueReportDto dto)
        {
            try
            {
                ReportResult? result = null;
                await foreach (var item in _context.Database
                    .SqlQueryRaw<ReportResult>(
                        "EXEC GenerateMonthlyRevenueReport @p_user_id, @p_year, @p_month",
                        new SqlParameter("p_user_id", dto.UserId),
                        new SqlParameter("p_year", dto.Year),
                        new SqlParameter("p_month", dto.Month))
                    .AsAsyncEnumerable())
                {
                    result = item;
                    break;
                }

                return Ok(new { result?.Message, result?.ReportId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}