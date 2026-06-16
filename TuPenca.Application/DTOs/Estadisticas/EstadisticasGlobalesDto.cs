using TuPenca.Domain.Enums;

namespace TuPenca.Application.DTOs.Estadisticas
{
    public class EstadisticasGlobalesDto
    {
        public int TotalSitios { get; set; }
        public int TotalUsuarios { get; set; }
        public int TotalPencasActivas { get; set; }
        public int TotalPencasFinalizadas { get; set; }
        public int TotalRecaudado { get; set; }
        public int TotalComisionesGeneradas { get; set; }
        public List<EstadisticaSitioResumenDto> TopSitiosPorUsuarios { get; set; } = new();
        public List<EstadisticaSitioResumenDto> TopSitiosPorRecaudacion { get; set; } = new();
        public List<EstadisticaSitioDetalleDto> EstadisticasPorSitio { get; set; } = new();
    }

    public class EstadisticaSitioResumenDto
    {
        public Guid SitioId { get; set; }
        public string NombreSitio { get; set; } = null!;
        public int Valor { get; set; }
    }

    public class EstadisticaSitioDetalleDto
    {
        public Guid SitioId { get; set; }
        public string NombreSitio { get; set; } = null!;
        public string UrlPropia { get; set; } = null!;
        public EstadoSitio Estado { get; set; }
        public int TotalUsuarios { get; set; }
        public int UsuariosPendientes { get; set; }
        public int TotalPencasActivas { get; set; }
        public int TotalPencasFinalizadas { get; set; }
        public int TotalInscripciones { get; set; }
        public int TotalRecaudado { get; set; }
        public int TotalComisiones { get; set; }
    }
}