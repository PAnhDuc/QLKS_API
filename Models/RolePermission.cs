using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS_API.Models
{
    [Table("Role_Permissions")]
    public class RolePermission
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public Role Role { get; set; } = null!;
        public Permission Permission { get; set; } = null!;
    }
}