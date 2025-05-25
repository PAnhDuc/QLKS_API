using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKS_API.Data;
using QLKS_API.DTOs;
using QLKS_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using BCrypt.Net;
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
            var user = await _context.Users
                .Include(u => u.Role)
                .SingleOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Tên người dùng hoặc mật khẩu không đúng");

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(dto.Username))
                return BadRequest(new { Message = "Tên người dùng là bắt buộc" });

            if (dto.Username.Length > 50)
                return BadRequest(new { Message = "Tên người dùng không được vượt quá 50 ký tự" });

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { Message = "Mật khẩu là bắt buộc" });

            if (dto.Password.Length < 6)
                return BadRequest(new { Message = "Mật khẩu phải có ít nhất 6 ký tự" });

            if (string.IsNullOrWhiteSpace(dto.FullName))
                return BadRequest(new { Message = "Họ và tên là bắt buộc" });

            if (dto.FullName.Length > 100)
                return BadRequest(new { Message = "Họ và tên không được vượt quá 100 ký tự" });

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email.Length > 100)
                return BadRequest(new { Message = "Email không được vượt quá 100 ký tự" });

            if (!string.IsNullOrWhiteSpace(dto.Phone) && dto.Phone.Length > 15)
                return BadRequest(new { Message = "Số điện thoại không được vượt quá 15 ký tự" });

            // Kiểm tra username đã tồn tại
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest(new { Message = "Tên người dùng đã tồn tại" });

            // Kiểm tra email đã tồn tại
            if (!string.IsNullOrWhiteSpace(dto.Email) && await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { Message = "Email đã tồn tại" });

            // Kiểm tra phone đã tồn tại
            if (!string.IsNullOrWhiteSpace(dto.Phone) && await _context.Users.AnyAsync(u => u.Phone == dto.Phone))
                return BadRequest(new { Message = "Số điện thoại đã tồn tại" });

            // Kiểm tra vai trò mặc định (RoleId = 4) có tồn tại không
            if (!await _context.Roles.AnyAsync(r => r.RoleId == 4))
                return StatusCode(500, new { Message = "Không thể đăng ký người dùng. Vai trò mặc định (RoleId = 4) không tồn tại trong cơ sở dữ liệu" });

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
                Console.WriteLine($"Lỗi DbUpdateException khi đăng ký người dùng: {errorMessage}");

                // Kiểm tra lỗi unique constraint để trả về thông báo cụ thể
                if (errorMessage.Contains("UNIQUE") || errorMessage.Contains("unique"))
                {
                    if (errorMessage.Contains("Email"))
                        return BadRequest(new { Message = "Email đã tồn tại" });
                    if (errorMessage.Contains("Phone"))
                        return BadRequest(new { Message = "Số điện thoại đã tồn tại" });
                    if (errorMessage.Contains("Username"))
                        return BadRequest(new { Message = "Tên người dùng đã tồn tại" });
                }
                // Trả về lỗi chuẩn JSON cho client
                return StatusCode(500, new { Message = "Không thể đăng ký người dùng. Lỗi cơ sở dữ liệu.", Detail = errorMessage });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine($"Lỗi không mong muốn khi đăng ký người dùng: {errorMessage}");
                // Luôn trả về object JSON, không trả về chuỗi
                return StatusCode(500, new { Message = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại sau.", Detail = errorMessage });
            }
        }

        [HttpPost("forgot-password/send-code")]
        public async Task<IActionResult> SendResetCode([FromBody] ForgotPasswordDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (user != null)
            {
                var code = new Random().Next(100000, 999999).ToString();
                _context.PasswordResetCodes.Add(new PasswordResetCode
                {
                    Email = dto.Email,
                    Code = code,
                    ExpiredAt = DateTime.UtcNow.AddMinutes(10)
                });
                await _context.SaveChangesAsync();
                await SendResetEmail(dto.Email, code); // Hàm gửi email
            }
            return Ok(new { Message = "Nếu email hợp lệ, mã xác thực đã được gửi." });
        }

        [HttpPost("forgot-password/verify-and-reset")]
        public async Task<IActionResult> VerifyCodeAndResetPassword([FromBody] ResetPasswordWithCodeDto dto)
        {
            // Tìm code hợp lệ
            var codeEntry = await _context.PasswordResetCodes
                .Where(x => x.Email == dto.Email && x.Code == dto.Code && x.ExpiredAt > DateTime.UtcNow)
                .OrderByDescending(x => x.ExpiredAt)
                .FirstOrDefaultAsync();

            if (codeEntry == null)
                return BadRequest(new { Message = "Mã xác thực không đúng hoặc đã hết hạn." });

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return BadRequest(new { Message = "Không tìm thấy người dùng." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            // Xoá code vừa dùng
            _context.PasswordResetCodes.Remove(codeEntry);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đặt lại mật khẩu thành công!" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                return StatusCode(500, new { Message = "Lỗi cấu hình máy chủ: Thiếu khóa JWT" });
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
                    return BadRequest(new { Message = "Token không hợp lệ" });
            }
            catch (SecurityTokenExpiredException)
            {
                return BadRequest(new { Message = "Token đã hết hạn" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Token không hợp lệ: {ex.Message}" });
            }

            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return NotFound(new { Message = "Không tìm thấy người dùng" });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đặt lại mật khẩu thành công" });
        }

        private string GenerateResetToken(string email)
        {
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("Khóa JWT chưa được cấu hình");
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
                expires: DateTime.Now.AddHours(1),
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
            {
                throw new InvalidOperationException("Cổng SMTP không hợp lệ trong cài đặt email");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Yêu Cầu Đặt Lại Mật Khẩu";

            message.Body = new TextPart("plain")
            {
                Text = $"Vui lòng nhấp vào liên kết sau để đặt lại mật khẩu của bạn: {resetLink}\nLiên kết này sẽ hết hạn sau 1 giờ."
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
                throw new InvalidOperationException("Khóa JWT chưa được cấu hình");
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