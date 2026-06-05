using System;
using System.Collections.Generic;
using System.Text;

namespace SV.Common.DTOs
{
    public class CreateUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public string? Country { get; set; }
    }
}
