using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKS_API.Data;
using QLKS_API.Models;
using QLKS_API.DTOs;

namespace QLKS_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Accountant")]
    public class InvoiceController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public InvoiceController(HotelDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoices()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Booking)
                .Include(i => i.Customer)
                .Select(invoice => new InvoiceDto
                {
                    InvoiceId = invoice.InvoiceId,
                    BookingId = invoice.BookingId,
                    CustomerId = invoice.CustomerId,
                    Status = (int)invoice.Status,
                    TotalAmount = invoice.TotalAmount,
                    CreatedAt = invoice.CreatedAt
                    // Map thêm trường nếu cần
                })
                .ToListAsync();

            return Ok(invoices);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Booking)
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null) return NotFound();

            var dto = new InvoiceDto
            {
                InvoiceId = invoice.InvoiceId,
                BookingId = invoice.BookingId,
                CustomerId = invoice.CustomerId,
                Status = (int)invoice.Status,
                TotalAmount = invoice.TotalAmount,
                CreatedAt = invoice.CreatedAt
                // Map thêm trường nếu cần
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateInvoice([FromBody] InvoiceDto dto)
        {
            var invoice = new Invoice
            {
                BookingId = dto.BookingId,
                CustomerId = dto.CustomerId,
                TotalAmount = dto.TotalAmount,
                Status = (InvoiceStatus)dto.Status,
                CreatedAt = DateTime.UtcNow
            };
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            var result = new InvoiceDto
            {
                InvoiceId = invoice.InvoiceId,
                BookingId = invoice.BookingId,
                CustomerId = invoice.CustomerId,
                Status = (int)invoice.Status,
                TotalAmount = invoice.TotalAmount,
                CreatedAt = invoice.CreatedAt
            };

            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.InvoiceId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice(int id, [FromBody] InvoiceDto dto)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return NotFound();

            invoice.BookingId = dto.BookingId;
            invoice.CustomerId = dto.CustomerId;
            invoice.TotalAmount = dto.TotalAmount;
            invoice.Status = (InvoiceStatus)dto.Status; // Explicit cast from int to InvoiceStatus
            invoice.CreatedAt = dto.CreatedAt;

            await _context.SaveChangesAsync();

            var result = new InvoiceDto
            {
                InvoiceId = invoice.InvoiceId,
                BookingId = invoice.BookingId,
                CustomerId = invoice.CustomerId,
                Status = (int)invoice.Status,
                TotalAmount = invoice.TotalAmount,
                CreatedAt = invoice.CreatedAt
            };

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return NotFound();
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}