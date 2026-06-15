using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Admin.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string Rol { get; set; } = null!;
        public DateTime Expira { get; set; }
    }
}
