using TuPenca.Admin.Models.Enums;

namespace TuPenca.Admin.DTOs.Estadisticas;

public class EstadisticasGlobalesFiltroDto
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public EstadoSitio? EstadoSitio { get; set; }
    public string? Buscar { get; set; }
}
