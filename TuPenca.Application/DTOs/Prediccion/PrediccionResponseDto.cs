using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Application.DTOs.Prediccion
{
    public class PrediccionResponseDto
    {
        public Guid Id { get; set; }
        public Guid PartidoId { get; set; }
        public string EquipoLocal { get; set; } = null!;
        public string EquipoVisitante { get; set; } = null!;
        public int GolesLocal { get; set; }
        public int GolesVisitante { get; set; }
        public DateTime FechaPartido { get; set; }
    }
}