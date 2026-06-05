using System;
using System.Collections.Generic;
using System.Text;

namespace SV.Common.DTOs
{
    public class UserResponseDto
    {
        public string UserGuid { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}
