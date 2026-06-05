using System;
using System.Collections.Generic;
using System.Text;

namespace SV.Common.DTOs.User
{
    public class UpdateUserDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public List<int>? RoleIds { get; set; }
    }
}
