using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Admin.DTOs.Partido
{
    public class ResultadoResponseDto
    {
        public Guid PartidoId { get; set; }
        public string EquipoLocal { get; set; } = null!;
        public string EquipoVisitante { get; set; } = null!;
        public int GolesLocal { get; set; }
        public int GolesVisitante { get; set; }
        public int UsuariosActualizados { get; set; }
    }
}