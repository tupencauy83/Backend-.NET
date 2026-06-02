using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TuPenca.Domain.Entities
{
    public class Partido : BaseEntity
    {
        public DateTime Fecha { get; set; }
        public string Fase { get; set; } = null!;
        public int? ResultadoLocal { get; set; }    // nullable hasta que termina
        public int? ResultadoVisitante { get; set; }

        // N Partidos → 1 EventoDeportivo
        public Guid EventoDeportivoId { get; set; }
        public EventoDeportivo Evento { get; set; } = null!;

        // ⚠️ Dos FKs al mismo Equipo — requiere [InverseProperty] en AppDbContext
        // N Partidos → 1 Equipo (como local)
        public Guid EquipoLocalId { get; set; }
        [InverseProperty("PartidosComoLocal")]
        public Equipo EquipoLocal { get; set; } = null!;

        // N Partidos → 1 Equipo (como visitante)
        public Guid EquipoVisitanteId { get; set; }
        [InverseProperty("PartidosComoVisitante")]
        public Equipo EquipoVisitante { get; set; } = null!;

        // Equipo ganador para ver resultado preciso en caso fuera a penales o algo
        public Guid? EquipoGanadorId { get; set; }
        public Equipo? EquipoGanador { get; set; }

        // 1 Partido → N Predicciones
        public ICollection<Prediccion> Predicciones { get; set; } = new List<Prediccion>();

        // Atributo para carga de resultados mediante API de proveedores

        public string? ExternalMatchId { get; set; }
    }
}
