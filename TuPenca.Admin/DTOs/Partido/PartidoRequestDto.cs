using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Admin.DTOs.Partido
{
    public class PartidoRequestDto
    {
        public DateTime Fecha { get; set; }
        public string Fase { get; set; } = null!;
        public Guid EquipoLocalId { get; set; }
        public Guid EquipoVisitanteId { get; set; }
        public Guid EventoDeportivoId { get; set; }
        // Atributo para carga de ID de api de provedor de resultados
        public string? ExternalMatchId { get; set; }
    }
}