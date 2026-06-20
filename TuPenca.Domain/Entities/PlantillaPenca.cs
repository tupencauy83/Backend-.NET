using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Domain.Entities
{
    public class PlantillaPenca : BaseEntity
    {
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public int TiempoLimitePrevioMinutos { get; set; } = 60; // default 1 hora

        // N PlantillasPenca → 1 EventoDeportivo
        public Guid EventoDeportivoId { get; set; }
        public EventoDeportivo Evento { get; set; } = null!;

        // 1 PlantillaPenca → N ReglasPuntuacion
        public ICollection<ReglaPuntuacion> Reglas { get; set; } = new List<ReglaPuntuacion>();

        // 1 PlantillaPenca → N Pencas
        public ICollection<Penca> Pencas { get; set; } = new List<Penca>();

        
        public int PuntajeGanador { get; set; }

        // Atributos para premios

        public int MontoEntrada { get; set; }
        public int PorcentajeComision { get; set; }
    }
}
