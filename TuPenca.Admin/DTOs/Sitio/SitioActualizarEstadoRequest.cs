using TuPenca.Admin.Models.Enums;

namespace TuPenca.Admin.DTOs.Sitio
{
    public class SitioActualizarEstadoRequest
    {
        public Guid Id { get; set; }
        public EstadoSitio Estado { get; set; }
    }
}
