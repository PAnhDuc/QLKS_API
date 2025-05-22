using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using QLKS_API.Data;
using QLKS_API.Models;
using QLKS_API.Models.Dtos;

namespace QLKS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly HotelDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger; // Khai báo logger

        public AuthController(HotelDbContext context, IConfiguration config, ILogger<AuthController> logger)
        {
            _context = context;
            _config = config;
            _logger = logger; // Gán logger từ dependency injection
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .SingleOrDefaultAsync(u => u.Username == dto.Username && u.PasswordHash == dto.PasswordHash);

            if (user == null)
                return Unauthorized();

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return NotFound(new { Message = "User not found" });

            var resetToken = Guid.NewGuid().ToString();
            // TODO: Lưu token và gửi email
            return Ok(new { Message = "Password reset link sent to email" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return NotFound(new { Message = "User not found" });

            user.PasswordHash = dto.NewPassword; // Thay bằng mã hóa mật khẩu
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Password reset successfully" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Username))
                    return BadRequest(new { Message = "Username cannot be empty" });

                if (string.IsNullOrWhiteSpace(dto.Email))
                    return BadRequest(new { Message = "Email cannot be empty" });

                if (string.IsNullOrWhiteSpace(dto.Password))
                    return BadRequest(new { Message = "Password cannot be empty" });

                if (string.IsNullOrWhiteSpace(dto.FullName))
                    return BadRequest(new { Message = "Full name cannot be empty" });

                if (dto.RoleId <= 0)
                    return BadRequest(new { Message = "RoleId must be greater than 0" });

                if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                    return BadRequest(new { Message = "Username already exists" });

                if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                    return BadRequest(new { Message = "Email already exists" });

                var role = await _context.Roles.FindAsync(dto.RoleId);
                if (role == null)
                    return BadRequest(new { Message = $"Role with ID {dto.RoleId} does not exist" });

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                var user = new User
                {
                    Username = dto.Username,
                    PasswordHash = passwordHash,
                    Email = dto.Email,
                    FullName = dto.FullName,
                    Phone = dto.Phone,
                    RoleId = dto.RoleId,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Registration successful", UserId = user.UserId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering user"); // Sử dụng logger
                return StatusCode(500, new { Message = "An error occurred while registering the user", Details = ex.Message });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role.RoleName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }

    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int RoleId { get; set; }
    }
}