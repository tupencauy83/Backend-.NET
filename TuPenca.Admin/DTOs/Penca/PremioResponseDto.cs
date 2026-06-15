using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Admin.DTOs.Penca
{
    public class PremioResponseDto
    {
        public int Posicion { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public int Monto { get; set; }
        public string Descripcion { get; set; } = null!;
    }
}