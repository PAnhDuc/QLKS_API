using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ChangePasswordDto
{
    public string Email { get; set; }
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}