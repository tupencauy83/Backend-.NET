using TuPenca.Admin.Models.Enums;

namespace TuPenca.Admin.DTOs.Sitio
{
    public class SitioRequestDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string UrlPropia { get; set; } = null!;
        public string ColorPrimario { get; set; } = null!;
        public string ColorSecundario { get; set; } = null!;
        public string? ConfiguracionSitio { get; set; } = null!;
        public TipoRegistro TipoRegistro { get; set; } = TipoRegistro.Abierta;
        public EstadoSitio Estado { get; set; } = EstadoSitio.Pendiente;
        public byte[]? Logo { get; set; } = null;
    }
}
