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
    [Authorize(Roles = "Admin,Manager,Receptionist")]
    public class RoomController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public RoomController(HotelDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetRooms()
        {
            var rooms = await _context.Rooms.ToListAsync();
            return Ok(rooms);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            return Ok(room);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] RoomDto dto)
        {
            var room = new Room
            {
                RoomNumber = dto.RoomNumber,
                RoomType = dto.RoomType,
                Price = dto.Price,
                Status = dto.Status,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl ?? "https://images.unsplash.com/photo-1506744038136-46273834b3fb" // Link ảnh mặc định
            };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRoom), new { id = room.RoomId }, room);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] RoomDto dto)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            room.RoomNumber = dto.RoomNumber;
            room.RoomType = dto.RoomType;
            room.Price = dto.Price;
            room.Status = dto.Status;
            room.Description = dto.Description;
            room.ImageUrl = dto.ImageUrl; // Thêm dòng này
            await _context.SaveChangesAsync();
            return Ok(room);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}