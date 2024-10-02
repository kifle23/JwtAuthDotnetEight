using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthDotnetEight.Models
{
    public class UserRole
    {
        public int UserId { get; set; }
        public required User User { get; set; }

        public int RoleId { get; set; }
        public required Role Role { get; set; }

    }
}
