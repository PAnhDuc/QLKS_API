using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKS_API.Data;
using QLKS_API.Models;
using QLKS_API.DTOs;

namespace QLKS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Accountant")]
    public class InvoiceServiceController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public InvoiceServiceController(HotelDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddInvoiceService([FromBody] InvoiceServiceDto dto)
        {
            var invoiceService = new InvoiceService
            {
                InvoiceId = dto.InvoiceId,
                ServiceId = dto.ServiceId,
                Quantity = dto.Quantity
            };
            _context.InvoiceServices.Add(invoiceService);
            await _context.SaveChangesAsync();
            return Ok(invoiceService);
        }

        [HttpDelete("{invoiceId}/{serviceId}")]
        public async Task<IActionResult> RemoveInvoiceService(int invoiceId, int serviceId)
        {
            var invoiceService = await _context.InvoiceServices
                .FindAsync(invoiceId, serviceId);
            if (invoiceService == null) return NotFound();
            _context.InvoiceServices.Remove(invoiceService);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}