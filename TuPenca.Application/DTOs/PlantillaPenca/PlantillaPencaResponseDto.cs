using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Application.DTOs.PlantillaPenca
{
    public class PlantillaPencaResponseDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public int TiempoLimitePrevioMinutos { get; set; }
        public string EventoDeportivo { get; set; } = null!;
        public DateTime? EventoFechaInicio { get; set; }
        public int MontoEntrada { get; set; }
        public int PorcentajeComision { get; set; }
        public int PuntajeGanador { get; set; }
        public List<ReglaPuntuacionDto> Reglas { get; set; } = new();
    }
}
