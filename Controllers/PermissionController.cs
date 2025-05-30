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
    [Authorize(Roles = "Admin,Manager")]
    public class PermissionController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public PermissionController(HotelDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPermissions()
        {
            var permissions = await _context.Permissions.ToListAsync();
            return Ok(permissions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPermission(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) return NotFound();
            return Ok(permission);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePermission([FromBody] PermissionDto dto)
        {
            var permission = new Permission { PermissionName = dto.PermissionName, Description = dto.Description };
            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPermission), new { id = permission.PermissionId }, permission);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePermission(int id, [FromBody] PermissionDto dto)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) return NotFound();
            permission.PermissionName = dto.PermissionName;
            permission.Description = dto.Description;
            await _context.SaveChangesAsync();
            return Ok(permission);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) return NotFound();
            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}