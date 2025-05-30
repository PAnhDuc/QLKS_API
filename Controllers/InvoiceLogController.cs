using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKS_API.Data;
using QLKS_API.DTOs;

namespace QLKS_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Accountant")]
    public class InvoiceLogController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public InvoiceLogController(HotelDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoiceLogs()
        {
            var logs = await _context.InvoiceLogs
                .Select(l => new InvoiceLogDto
                {
                    LogId = l.LogId,
                    Status = l.Status,
                    ChangedAt = l.ChangedAt
                })
                .ToListAsync();
            return Ok(logs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoiceLog(int id)
        {
            var log = await _context.InvoiceLogs
                .Where(l => l.LogId == id)
                .Select(l => new InvoiceLogDto
                {
                    LogId = l.LogId,
                    Status = l.Status,
                    ChangedAt = l.ChangedAt
                    // Không có InvoiceId
                })
                .FirstOrDefaultAsync();
            if (log == null) return NotFound();
            return Ok(log);
        }
    }
}