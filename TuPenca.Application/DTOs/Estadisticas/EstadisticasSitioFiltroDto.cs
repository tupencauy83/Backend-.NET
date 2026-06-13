using TuPenca.Domain.Enums;

namespace TuPenca.Application.DTOs.Estadisticas
{
    public class EstadisticasSitioFiltroDto
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public EstadoPenca? EstadoPenca { get; set; }
        public string? Buscar { get; set; }
    }
}
