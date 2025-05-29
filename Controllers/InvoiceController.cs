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
                    Status = invoice.Status,
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
                Status = invoice.Status,
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
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow
            };
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            var result = new InvoiceDto
            {
                InvoiceId = invoice.InvoiceId,
                BookingId = invoice.BookingId,
                CustomerId = invoice.CustomerId,
                Status = invoice.Status,
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

            // Update bằng SQL thuần để tránh OUTPUT clause
            var sql = "UPDATE Invoices SET booking_id = @p0, customer_id = @p1, total_amount = @p2, status = @p3 WHERE invoice_id = @p4";
            await _context.Database.ExecuteSqlRawAsync(sql, dto.BookingId, dto.CustomerId, dto.TotalAmount, dto.Status, id);

            // Lấy lại dữ liệu mới
            var updated = await _context.Invoices.FindAsync(id);
            var result = new InvoiceDto
            {
                InvoiceId = updated.InvoiceId,
                BookingId = updated.BookingId,
                CustomerId = updated.CustomerId,
                Status = updated.Status,
                TotalAmount = updated.TotalAmount,
                CreatedAt = updated.CreatedAt
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