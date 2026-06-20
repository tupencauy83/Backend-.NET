using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Application.DTOs.PlantillaPenca
{
    public class PlantillaPencaRequestDto
    {
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public int TiempoLimitePrevioMinutos { get; set; }
        public Guid EventoDeportivoId { get; set; }
        public List<ReglaPuntuacionDto> Reglas { get; set; } = new();

        public int PuntajeGanador { get; set; }

        // atributos para premios

        public int MontoEntrada { get; set; }
        public int PorcentajeComision { get; set; }
    }

    public class ReglaPuntuacionDto
    {
        public int Desviacion { get; set; }
        public int Puntaje { get; set; }
    }
}