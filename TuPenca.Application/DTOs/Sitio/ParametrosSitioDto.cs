using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Application.DTOs.ParametrosSitio
{
    public class ParametrosSitioResponseDto
    {
        public Guid Id { get; set; }
        public Guid SitioId { get; set; }
        public bool NotifRecordatorioPrediccion { get; set; }
        public int HorasAntesRecordatorio { get; set; }
        public bool NotifResumenSemanal { get; set; }
        public DayOfWeek DiaResumenSemanal { get; set; }
        public int HoraResumenSemanal { get; set; }
        public bool NotifResultadoPartido { get; set; }
    }

    public class ActualizarParametrosSitioDto
    {
        public bool? NotifRecordatorioPrediccion { get; set; }
        public int? HorasAntesRecordatorio { get; set; }
        public bool? NotifResumenSemanal { get; set; }
        public DayOfWeek? DiaResumenSemanal { get; set; }
        public int? HoraResumenSemanal { get; set; }
        public bool? NotifResultadoPartido { get; set; }
    }
}