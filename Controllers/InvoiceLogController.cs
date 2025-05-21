using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKS_API.Data;

namespace QLKS_API.Controllers
{
    [Route("api/[controller]")]
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
            var logs = await _context.InvoiceLogs.Include(l => l.Invoice).ToListAsync();
            return Ok(logs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoiceLog(int id)
        {
            var log = await _context.InvoiceLogs.Include(l => l.Invoice).FirstOrDefaultAsync(l => l.LogId == id);
            if (log == null) return NotFound();
            return Ok(log);
        }
    }
}