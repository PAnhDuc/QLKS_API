using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS_API.DTOs
{
    public class PermissionDto
    {
        public string PermissionName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}