using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Admin.DTOs.Evento
{
    public class EventoRequestDto
    {
        public string Nombre { get; set; } = null!;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public Guid DeporteId { get; set; }
        public Guid TipoCompetenciaId { get; set; }
    }
}