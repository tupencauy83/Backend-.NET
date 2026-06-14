using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Domain.Entities
{
    public class ParametrosSitio : BaseEntity
    {
        public Guid SitioId { get; set; }
        public Sitio Sitio { get; set; } = null!;

        // Recordatorio de predicción
        public bool NotifRecordatorioPrediccion { get; set; } = true;
        public int HorasAntesRecordatorio { get; set; } = 2;

        // Resumen semanal
        public bool NotifResumenSemanal { get; set; } = true;
        public DayOfWeek DiaResumenSemanal { get; set; } = DayOfWeek.Friday;
        public int HoraResumenSemanal { get; set; } = 9; // UTC, 0-23

        // Resultado de partido
        public bool NotifResultadoPartido { get; set; } = true;
    }
}