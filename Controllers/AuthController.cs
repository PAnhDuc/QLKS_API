using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKS_API.Data;
using QLKS_API.DTOs;
using QLKS_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using MailKit.Net.Smtp;
using MimeKit;

namespace QLKS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly HotelDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(HotelDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .SingleOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid username or password");

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            {
                return BadRequest(new { Message = "Username already exists" });
            }

            var user = new User
            {
                RoleId = dto.RoleId,
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User registered successfully", UserId = user.UserId });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return NotFound(new { Message = "Email not found" });

            // Tạo mã đặt lại mật khẩu bằng JWT
            var resetToken = GenerateResetToken(dto.Email);
            
            // Gửi email với liên kết đặt lại
            var resetLink = $"http://localhost:5000/api/Auth/reset-password?token={resetToken}&email={dto.Email}";
            try
            {
                await SendResetEmail(dto.Email, resetLink);
            }
            catch (Exception ex)
            {
                // Log lỗi (trong thực tế, bạn nên dùng ILogger)
                Console.WriteLine($"Failed to send reset email: {ex.Message}");
                return StatusCode(500, new { Message = "Failed to send reset email. Please try again later." });
            }

            return Ok(new { Message = "Password reset link has been sent to your email" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            // Xác thực token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                return StatusCode(500, new { Message = "Server configuration error: JWT Key is missing." });
            }

            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                tokenHandler.ValidateToken(dto.Token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var emailClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "email")?.Value;

                if (emailClaim != dto.Email)
                    return BadRequest(new { Message = "Invalid token" });
            }
            catch (SecurityTokenExpiredException)
            {
                return BadRequest(new { Message = "Token has expired" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Invalid token: {ex.Message}" });
            }

            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return NotFound(new { Message = "User not found" });

            // Đặt lại mật khẩu
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Password has been reset successfully" });
        }

        private string GenerateResetToken(string email)
        {
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is not configured.");
            }

            var claims = new[]
            {
                new Claim("email", email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1), // Hết hạn sau 1 giờ
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task SendResetEmail(string email, string resetLink)
        {
            var emailSettings = _config.GetSection("EmailSettings");
            
            // Kiểm tra cấu hình email
            var smtpServer = emailSettings["SmtpServer"];
            var smtpPortStr = emailSettings["SmtpPort"];
            var senderEmail = emailSettings["SenderEmail"];
            var senderName = emailSettings["SenderName"];
            var username = emailSettings["Username"];
            var password = emailSettings["Password"];

            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpPortStr) ||
                string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderName) ||
                string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("Email settings are not properly configured in appsettings.json.");
            }

            if (!int.TryParse(smtpPortStr, out int smtpPort))
            {
                throw new InvalidOperationException("Invalid SMTP port in email settings.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Password Reset Request";

            message.Body = new TextPart("plain")
            {
                Text = $"Please click the following link to reset your password: {resetLink}\nThis link will expire in 1 hour."
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is not configured.");
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role.RoleName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
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
}