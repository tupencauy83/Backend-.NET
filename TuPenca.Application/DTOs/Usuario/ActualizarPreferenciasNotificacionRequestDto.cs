using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Application.DTOs.Usuario
{
    public class ActualizarPreferenciasNotificacionRequestDto
    {
        public bool NotifRecordatorioPrediccion { get; set; }
        public bool NotifResultadoPartido { get; set; }
        public bool NotifResumenSemanal { get; set; }
    }
}