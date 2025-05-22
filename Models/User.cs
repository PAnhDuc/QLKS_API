using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace QLKS_API.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; } // Đánh dấu UserId là khóa chính
        public int RoleId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public Role Role { get; set; } = null!;
        public List<Customer> Customers { get; set; } = new();
        public List<Report> Reports { get; set; } = new();
    }
}