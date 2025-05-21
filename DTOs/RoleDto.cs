using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS_API.DTOs
{
    public class RoleDto
    {
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}