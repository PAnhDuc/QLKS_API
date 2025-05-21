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
    [Authorize(Roles = "Admin")]
    public class RolePermissionController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public RolePermissionController(HotelDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AssignPermission([FromBody] RolePermissionDto dto)
        {
            var rolePermission = new RolePermission
            {
                RoleId = dto.RoleId,
                PermissionId = dto.PermissionId
            };
            _context.RolePermissions.Add(rolePermission);
            await _context.SaveChangesAsync();
            return Ok(rolePermission);
        }

        [HttpDelete("{roleId}/{permissionId}")]
        public async Task<IActionResult> RemovePermission(int roleId, int permissionId)
        {
            var rolePermission = await _context.RolePermissions
                .FindAsync(roleId, permissionId);
            if (rolePermission == null) return NotFound();
            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}