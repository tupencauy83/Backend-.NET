using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Application.DTOs.Usuario
{
    public class PreferenciasNotificacionResponseDto
    {
        public bool NotifRecordatorioPrediccion { get; set; }
        public bool NotifResultadoPartido { get; set; }
        public bool NotifResumenSemanal { get; set; }
    }
}