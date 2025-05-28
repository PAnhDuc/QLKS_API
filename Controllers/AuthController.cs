using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKS_API.Data;
using QLKS_API.DTOs;
using QLKS_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MailKit.Net.Smtp;
using MimeKit;
using QLKS_API.Models.Dtos;

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
            var user = await _context.Users.Include(u => u.Role)
                .SingleOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Tên người dùng hoặc mật khẩu không đúng");

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var inputError = await ValidateRegisterInput(dto);
            if (inputError != null) return BadRequest(new { Message = inputError });

            // Kiểm tra username/email/phone đã tồn tại
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest(new { Message = "Tên người dùng đã tồn tại" });
            if (!string.IsNullOrWhiteSpace(dto.Email) && await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { Message = "Email đã tồn tại" });
            if (!string.IsNullOrWhiteSpace(dto.Phone) && await _context.Users.AnyAsync(u => u.Phone == dto.Phone))
                return BadRequest(new { Message = "Số điện thoại đã tồn tại" });

            // Đảm bảo vai trò mặc định tồn tại
            if (!await _context.Roles.AnyAsync(r => r.RoleId == 4))
                return StatusCode(500, new { Message = "Vai trò mặc định không tồn tại" });

            try
            {
                var user = new User
                {
                    RoleId = 4,
                    Username = dto.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    FullName = dto.FullName,
                    Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email,
                    Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Đăng ký người dùng thành công", UserId = user.UserId });
            }
            catch (DbUpdateException ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                if (errorMessage.Contains("UNIQUE") || errorMessage.Contains("unique"))
                {
                    if (errorMessage.Contains("Email"))
                        return BadRequest(new { Message = "Email đã tồn tại" });
                    if (errorMessage.Contains("Phone"))
                        return BadRequest(new { Message = "Số điện thoại đã tồn tại" });
                    if (errorMessage.Contains("Username"))
                        return BadRequest(new { Message = "Tên người dùng đã tồn tại" });
                }
                return StatusCode(500, new { Message = "Không thể đăng ký người dùng. Lỗi cơ sở dữ liệu.", Detail = errorMessage });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Đã xảy ra lỗi không mong muốn.", Detail = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);
            // Vẫn trả về OK để tránh lộ email tồn tại
            if (user != null)
            {
                var token = GenerateResetToken(user.Email);
                var link = $"http://localhost:5000/reset-password?token={token}";
                await SendResetEmail(user.Email, link);
            }
            return Ok(new { Message = "Nếu email hợp lệ, link đặt lại mật khẩu đã được gửi." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordWithTokenDto dto)
        {
            var payload = ValidateResetToken(dto.Token);
            if (payload == null)
                return BadRequest(new { Message = "Token không hợp lệ hoặc đã hết hạn." });

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == payload.Email);
            if (user == null)
                return BadRequest(new { Message = "Không tìm thấy người dùng." });

            await ChangeUserPassword(user, dto.NewPassword);
            return Ok(new { Message = "Đặt lại mật khẩu thành công!" });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return NotFound(new { Message = "Không tìm thấy người dùng" });

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
                return BadRequest(new { Message = "Mật khẩu cũ không đúng" });

            await ChangeUserPassword(user, dto.NewPassword);
            return Ok(new { Message = "Đổi mật khẩu thành công" });
        }

        // ---- PRIVATE SUPPORT METHODS ----

        private async Task ChangeUserPassword(User user, string newPassword)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordChangedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        private async Task<string?> ValidateRegisterInput(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
                return "Tên người dùng là bắt buộc";
            if (dto.Username.Length > 50)
                return "Tên người dùng không được vượt quá 50 ký tự";
            if (string.IsNullOrWhiteSpace(dto.Password))
                return "Mật khẩu là bắt buộc";
            if (dto.Password.Length < 6)
                return "Mật khẩu phải có ít nhất 6 ký tự";
            if (string.IsNullOrWhiteSpace(dto.FullName))
                return "Họ và tên là bắt buộc";
            if (dto.FullName.Length > 100)
                return "Họ và tên không được vượt quá 100 ký tự";
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email.Length > 100)
                return "Email không được vượt quá 100 ký tự";
            if (!string.IsNullOrWhiteSpace(dto.Phone) && dto.Phone.Length > 15)
                return "Số điện thoại không được vượt quá 15 ký tự";
            return null;
        }

        private class ResetTokenPayload { public string Email { get; set; } = ""; }

        private ResetTokenPayload? ValidateResetToken(string token)
        {
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey)) return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                var emailClaim = principal.Claims.FirstOrDefault(c => c.Type == "email");
                return emailClaim == null ? null : new ResetTokenPayload { Email = emailClaim.Value };
            }
            catch { return null; }
        }

        private string GenerateResetToken(string email)
        {
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("Khóa JWT chưa được cấu hình");

            var claims = new[]
            {
                new Claim("email", email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task SendResetEmail(string email, string resetLink)
        {
            var emailSettings = _config.GetSection("EmailSettings");
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
                throw new InvalidOperationException("Cài đặt email chưa được cấu hình đúng trong appsettings.json");
            }

            if (!int.TryParse(smtpPortStr, out int smtpPort))
                throw new InvalidOperationException("Cổng SMTP không hợp lệ trong cài đặt email");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Yêu Cầu Đặt Lại Mật Khẩu";
            message.Body = new TextPart("plain")
            {
                Text = $"Vui lòng nhấp vào liên kết sau để đặt lại mật khẩu của bạn: {resetLink}\nLiên kết này sẽ hết hạn sau 1 giờ."
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("Khóa JWT chưa được cấu hình");

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
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}