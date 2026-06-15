using System;
using System.Collections.Generic;
using System.Text;
using TuPenca.Admin.DTOs.Partido;

namespace TuPenca.Admin.DTOs.Evento
{
    public class EventoResponseDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = null!;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public List<PartidoResponseDto>? Partidos { get; set; }
    }
}
