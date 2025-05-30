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
    public class RoleController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public RoleController(HotelDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles.ToListAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();
            return Ok(role);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] RoleDto dto)
        {
            var role = new Role { RoleName = dto.RoleName, Description = dto.Description };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRole), new { id = role.RoleId }, role);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleDto dto)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();
            role.RoleName = dto.RoleName;
            role.Description = dto.Description;
            await _context.SaveChangesAsync();
            return Ok(role);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}