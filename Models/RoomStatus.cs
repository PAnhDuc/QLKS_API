using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace QLKS_API.Models
{
    public enum RoomStatus
    {
        Available = 0,
        Booked = 1,
        Cleaning = 2,
        Maintenance = 3,
        Unavailable = 4
    }
}